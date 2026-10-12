# Arquitectura — Mundialito de Fútbol Corporativo

Diagramas en [Mermaid](https://mermaid.js.org/): GitHub los dibuja automáticamente al abrir este archivo.

| Elemento pedido (§13) | Dónde se ve |
|---|---|
| Capas | Diagramas 1 y 2 |
| CQRS · EF Core (write) · Dapper (read) | Diagramas 2, 3 y 4 |
| UnitOfWork | Diagramas 2 y 3 |
| Idempotency storage | Diagramas 2 y 3 |
| Eventos | Diagramas 2 y 3 |
| Logs · Trace flow | Diagramas 2, 3, 4 y sección 5 |
| Frontend · Docker | Diagrama 2 |

---

## 1. Capas y dependencias (Clean Architecture)

Las dependencias apuntan siempre hacia el dominio. Domain no conoce a nadie; Infrastructure implementa los contratos que define Application; Api solo orquesta.

```mermaid
flowchart LR
    Api["Api<br/>controllers, middlewares,<br/>filtro de idempotencia"]
    Infrastructure["Infrastructure<br/>EF Core, Dapper, UnitOfWork,<br/>idempotency store, seed"]
    Application["Application<br/>commands, queries, handlers,<br/>contratos (interfaces)"]
    Domain["Domain<br/>entidades, reglas, Result,<br/>eventos de dominio"]

    Api --> Application
    Api --> Infrastructure
    Infrastructure --> Application
    Infrastructure --> Domain
    Application --> Domain
```

---

## 2. Vista general: componentes, CQRS y Docker

```mermaid
flowchart TB
    user(["Usuario en el navegador"])
    tester(["Postman / Swagger"])

    subgraph compose["Docker Compose"]
        direction TB

        subgraph fe["frontend · Next.js 16 · puerto 3000"]
            pages["Páginas<br/>Tabla · Goleadores · Partidos · Equipos"]
            svc["Capa de servicios<br/>apiRequest + Idempotency-Key"]
            pages --> svc
        end

        subgraph apiContainer["api · ASP.NET Core 8 · puerto 8080"]
            direction TB

            subgraph apiLayer["Capa Api (orquesta)"]
                mw["Middlewares<br/>CorrelationId / TraceId<br/>Serilog: log y duración por request<br/>Manejo global de errores"]
                idem["IdempotencyFilter<br/>POST con Idempotency-Key"]
                ctrl["Controllers<br/>Result → 200 / 201 / 400 / 404 / 409"]
            end

            subgraph appLayer["Capa Application"]
                cmd["Commands<br/>ICommandHandler"]
                qry["Queries<br/>IQueryHandler"]
            end

            subgraph domLayer["Capa Domain"]
                agg["Agregados Team y Match<br/>Result / Error · StandingRules"]
                evt["Eventos de dominio<br/>TeamCreated · MatchResultRegistered"]
            end

            subgraph infLayer["Capa Infrastructure"]
                repo["Repositorios<br/>EF Core"]
                uow["UnitOfWork<br/>una transacción + registro de eventos"]
                read["Read services<br/>Dapper · OFFSET/FETCH · whitelist de sortBy"]
                store["IdempotencyStore<br/>Dapper"]
                seed["Migraciones + Seed<br/>al arrancar"]
            end
        end

        subgraph dbContainer["db · SQL Server 2022 · puerto 1433"]
            data[("Teams · Players<br/>Matches · Goals")]
            idemTable[("IdempotencyRecords")]
            eventTable[("DomainEvents<br/>con TraceId")]
        end
    end

    logs[["Logs estructurados en consola<br/>TraceId + CorrelationId en cada línea"]]

    user --> pages
    svc -->|"HTTP / JSON"| mw
    tester -->|"HTTP / JSON"| mw
    mw --> idem --> ctrl

    ctrl -->|"escritura"| cmd
    ctrl -->|"lectura"| qry

    cmd --> agg
    agg -.->|"registra"| evt
    cmd --> repo
    cmd -->|"CommitAsync"| uow

    repo -->|"EF Core (write)"| data
    uow -->|"SaveChanges en transacción"| data
    uow -->|"mismo commit"| eventTable

    qry --> read
    read -->|"Dapper (read)"| data

    idem --> store --> idemTable
    seed --> data

    mw -.->|"log por request"| logs
    uow -.->|"log de eventos"| logs
```

- **CQRS:** los *commands* escriben con **EF Core** y siempre guardan con el **UnitOfWork**; las *queries* leen **solo con Dapper**, paginando en SQL.
- **Docker:** `docker compose up` levanta los 3 contenedores. La API espera a que SQL Server esté *healthy*, aplica las migraciones y carga el seed si la base está vacía.

---

## 3. Flujo de escritura: registrar un resultado

`POST /api/matches/:id/result` con el header `Idempotency-Key`.

```mermaid
sequenceDiagram
    autonumber
    actor U as Frontend / Postman
    participant MW as Middlewares
    participant IF as IdempotencyFilter
    participant ST as IdempotencyStore
    participant C as MatchesController
    participant H as RegisterMatchResult handler
    participant M as Match (dominio)
    participant UOW as UnitOfWork
    participant DB as SQL Server
    participant L as Logs (Serilog)

    U->>MW: POST /api/matches/:id/result + Idempotency-Key
    MW->>MW: asigna TraceId y CorrelationId (headers y contexto de logs)
    MW->>IF: continúa
    IF->>ST: busca la key
    alt key ya usada con el mismo payload
        ST-->>IF: respuesta guardada
        IF-->>U: la MISMA respuesta original (Idempotent-Replayed)
    else key ya usada con otro payload
        IF-->>U: 409 Idempotency.KeyReused
    else key nueva
        IF->>C: ejecuta el endpoint
        C->>H: RegisterMatchResultCommand
        H->>DB: carga el partido (EF Core)
        H->>M: RegisterResult(marcador, goleadores)
        alt regla de negocio incumplida
            M-->>H: Result.Failure (ej. Match.AlreadyPlayed)
            H-->>C: Result.Failure
            C-->>IF: 400 / 404 / 409 ProblemDetails con code y traceId
        else datos válidos
            M->>M: estado Played + evento MatchResultRegistered
            M-->>H: Result.Success
            H->>UOW: CommitAsync()
            UOW->>DB: BEGIN TRAN, guarda goles, partido y evento (con TraceId), COMMIT
            UOW->>L: Evento de dominio MatchResultRegistered registrado
            C-->>IF: 200 OK con el partido actualizado
        end
        IF->>ST: guarda key, huella del request, status y body
        IF-->>U: respuesta
    end
    MW->>L: HTTP POST respondió con su status y duración en ms
```

Si el `SaveChanges` falla, la transacción hace **rollback**: no se guardan ni los cambios ni los eventos.

---

## 4. Flujo de lectura: tabla de posiciones

`GET /api/standings?pageNumber=1&pageSize=10&sortBy=points&sortDirection=desc`

```mermaid
sequenceDiagram
    autonumber
    actor U as Frontend / Postman
    participant MW as Middlewares
    participant C as StandingsController
    participant H as GetStandings handler
    participant RS as StandingsReadService (Dapper)
    participant DB as SQL Server
    participant L as Logs (Serilog)

    U->>MW: GET /api/standings con paginación, orden y filtros
    MW->>C: request con TraceId y CorrelationId
    C->>H: GetStandingsQuery
    H->>H: valida página y sortBy contra la lista blanca
    alt parámetros inválidos
        H-->>C: Result.Failure (Paging.InvalidSortBy)
        C-->>U: 400 ProblemDetails
    else parámetros válidos
        H->>RS: GetStandingsAsync
        RS->>DB: SQL: agrega partidos jugados, puntos con StandingRules 3 / 1 / 0, ORDER BY y OFFSET/FETCH
        DB-->>RS: total de registros + solo la página pedida
        RS-->>H: PagedResult
        H-->>C: Result.Success
        C-->>U: 200 con data, pageNumber, pageSize, totalRecords y totalPages
    end
    H->>L: Query GetStandingsQuery completada en X ms
    MW->>L: HTTP GET respondió 200 en X ms
```

---

## 5. Trazabilidad (trace flow)

1. Cada request recibe un **TraceId** (W3C, del `Activity` de .NET) y un **CorrelationId** (el header `X-Correlation-Id` del cliente o, si no viene, el mismo TraceId).
2. Ambos se devuelven en los headers `X-Trace-Id` y `X-Correlation-Id`, y se agregan a **todas** las líneas de log del request.
3. Los **eventos de dominio** se guardan en la tabla `DomainEvents` con ese TraceId, en el mismo commit que los cambios.
4. Los errores responden con `traceId` en el ProblemDetails.

Con un solo TraceId se puede seguir un request completo: la respuesta HTTP, los logs del middleware y del handler, y los eventos guardados en la base.
