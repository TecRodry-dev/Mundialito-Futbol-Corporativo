# Mundialito de Fútbol Corporativo — Guía para Claude

Prueba técnica asíncrona. Entrega: lunes 12 de octubre de 2026, 9:00 a. m.
El enunciado completo está en @docs/ENUNCIADO.md — es la fuente de verdad. Si algo aquí contradice el enunciado, manda el enunciado.

## Estructura del repo
- `backend/` — solución .NET 8 (Clean Architecture)
  - `src/Mundialito.Domain` — entidades, value objects, eventos de dominio, `Result`/`Error` (en `Common/`). SIN dependencias a otras capas ni paquetes.
  - `src/Mundialito.Application` — commands, queries, handlers, interfaces (repositorios, UnitOfWork, read services). Depende SOLO de Domain y no usa paquetes externos.
  - `src/Mundialito.Infrastructure` — EF Core (DbContext, configuraciones, migraciones, repositorios, UnitOfWork), Dapper (queries), seed, idempotency store.
  - `src/Mundialito.Api` — controllers, middlewares (CorrelationId/TraceId, duración, idempotencia), mapeo Result → HTTP. Solo orquesta, sin lógica de negocio.
  - `tests/Mundialito.UnitTests` — xUnit.
- `frontend/` — Next.js (App Router, TypeScript). Capa de servicios/API separada de los componentes.
- `docker-compose.yml`, Dockerfiles, colección Postman en `docs/postman/`, diagrama en `docs/arquitectura/`.

## Base de datos: SQL Server
- Motor elegido: **SQL Server 2022** (imagen Docker `mcr.microsoft.com/mssql/server:2022-latest`).
- Escritura: EF Core con el provider `Microsoft.EntityFrameworkCore.SqlServer`. Lectura: Dapper sobre `Microsoft.Data.SqlClient`.
- SQL de las queries en dialecto T-SQL: paginación con `ORDER BY ... OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY`; siempre parámetros, nunca concatenar valores.
- La contraseña de `sa` (`MSSQL_SA_PASSWORD`) y la connection string van por variables de entorno (`.env`, no versionado); en el repo solo `.env.example` con valores de ejemplo.
- En `docker-compose`, la API debe esperar a que SQL Server esté sano (healthcheck) y aplicar migraciones + seed al iniciar.

## Decisiones de diseño (acordadas con el usuario)
- **`Result` vive en Domain** (`Domain/Common`), no en Application: el dominio necesita devolver fallos sin lanzar excepciones y no puede depender de otras capas. `Error` = mensaje + código opcional + `ErrorType` (Validation → 400, NotFound → 404, Conflict → 409).
- **Sin MediatR:** interfaces propias `ICommandHandler` / `IQueryHandler` en Application, registradas en DI; los controllers reciben el handler.
- **Ids `Guid`** generados por el dominio al crear la entidad (los eventos ya llevan el Id antes de guardar).
- **Modelo:**
  - `Team`: `Name` (único), `Coach`.
  - `Player`: `TeamId`, `FullName`, `ShirtNumber` (único por equipo), `Position` (Goalkeeper/Defender/Midfielder/Forward).
  - `Match`: `HomeTeamId`, `AwayTeamId` (distintos), `ScheduledAt`, `Venue` (opcional), `Status` (Scheduled/Played), `HomeGoals`/`AwayGoals` (null hasta jugarse).
  - `Goal`: `MatchId`, `PlayerId`, `TeamId`, `Minute` (opcional, 1–120). Los goles de cada equipo deben cuadrar con el marcador; no se contemplan autogoles.
  - Agregados: `Team` (con sus `Player`, que solo se crean con `team.AddPlayer`) y `Match` (con sus `Goal`, que solo se crean con `match.RegisterResult`).
  - Reglas de la tabla en `Domain/Standings` (`StandingRules`, `StandingsCalculator`, con tests). La consulta Dapper de la tabla debe usar las constantes de `StandingRules` como parámetros y el mismo orden.
  - `IdempotencyRecord`: `Key`, `RequestHash`, `StatusCode`, `ResponseBody`, `CreatedAt`.
  - La tabla de posiciones NO es tabla: se calcula con Dapper desde los partidos jugados.
- **Convención de idioma:** código (clases, métodos, propiedades) en inglés; mensajes al usuario, comentarios y commits en español.
- **Tests:** xUnit con `Assert` nativo (sin FluentAssertions: la v8 tiene licencia comercial).

## Reglas no negociables
- **CQRS:** Commands escriben con EF Core; Queries leen EXCLUSIVAMENTE con Dapper. Nunca usar EF para lectura en queries.
- **Unit of Work:** todo Command guarda vía `IUnitOfWork` (commit explícito, transacción). Prohibido llamar `DbContext.SaveChanges` fuera del UnitOfWork.
- **Result Pattern:** `Result` y `Result<T>` con Success/Failure, mensaje y código de error opcional. No usar excepciones como control de flujo (solo para fallos técnicos inesperados). La API mapea a 200/201/400/404/409.
- **REST:** GET lee, POST crea, PUT reemplaza (idempotente), PATCH parcial, DELETE idempotente.
- **Idempotencia en POST:** header `Idempotency-Key`, key persistida en BD, repetición devuelve la misma respuesta original (status + body); misma key con payload distinto → 409.
- **Paginación:** todos los GET aceptan `pageNumber`, `pageSize`, `sortBy`, `sortDirection` + filtros. Paginar en SQL (OFFSET/FETCH), nunca en memoria. `sortBy` validado contra una whitelist (evitar SQL injection). Respuesta: `{ data, pageNumber, pageSize, totalRecords, totalPages }`.
- **Observabilidad:** Serilog con logs estructurados (plantillas con propiedades, no strings concatenados), niveles correctos, sin datos sensibles. CorrelationId/TraceId por request en header y en todos los logs; registrar duración del request.
- **Eventos de dominio:** como mínimo `TeamCreated` y `MatchResultRegistered`; se registran, se loggean y llevan el TraceId.
- **Tabla de posiciones:** victoria 3, empate 1, derrota 0. Orden: puntos → diferencia de gol → goles a favor.
- **Seed** (en Infrastructure, solo si la BD está vacía, debe funcionar en Docker): 4 equipos, 5 jugadores por equipo, 6 partidos, 3 con resultado.
- **Secretos:** nada de contraseñas reales ni connection strings con secretos en el repo; usar variables de entorno / `.env.example`.

## Límites de ejecución (lo que Claude NO ejecuta)
- **Ningún comando `dotnet`, `npm`/`npx` ni `docker`:** ni build, ni test, ni restore, ni run, ni ef. Todo lo ejecuta el usuario. Claude solo escribe/edita archivos y entrega los comandos exactos para que el usuario los corra y le comparta la salida si hay errores.
- **No levantar el sistema:** Claude no ejecuta `dotnet run`, `dotnet watch`, `npm run dev`, `npm start`, `docker compose up`, `docker run` ni nada que arranque la API, el frontend o la base de datos.
- **No instalar librerías ni herramientas:** Claude no ejecuta `dotnet add package`, `dotnet tool install`, `npm install`/`npm i`, `npx` que descargue paquetes, ni `docker pull`/`docker build`. Cuando se necesite un paquete, Claude indica el comando exacto (paquete + versión + proyecto) y el usuario lo ejecuta.
- **Git: commits y push SOLO los hace el usuario.** Claude nunca ejecuta `git add`, `git commit`, `git push`, `git reset`, `git checkout`, `git merge` ni nada que modifique el repositorio o el remoto. Claude solo propone el mensaje de commit y puede usar comandos de lectura (`git status`, `git log`, `git diff`) para revisar.
- **Migraciones EF:** Claude escribe el código (DbContext, configuraciones) e indica el comando `dotnet ef migrations add ...`; el usuario lo ejecuta. Claude nunca ejecuta `dotnet ef database update`.
- Claude sí puede editar archivos de proyecto (`.csproj`, `package.json`) cuando el usuario lo pida, pero la instalación/restauración la hace el usuario.

## Forma de trabajar
- Tareas pequeñas y verificables. Al terminar cada cambio, Claude indica qué debe ejecutar el usuario para verificar: `dotnet build` y `dotnet test` desde `backend/`; `npm run build` / `npm run lint` desde `frontend/`.
- No hacer cambios arquitectónicos grandes, migraciones destructivas ni borrar archivos sin preguntar.
- Toda lógica de torneo (puntos, diferencia de gol, orden de tabla) debe tener pruebas unitarias.
- Al terminar cada bloque, explicar brevemente qué se hizo y por qué.
- Commits pequeños con mensajes claros en español (Claude los sugiere; el usuario los ejecuta).
- Responder en español.

## Comandos (todos los ejecuta SOLO el usuario)
- Backend: `cd backend && dotnet build && dotnet test`
- API local: `dotnet run --project src/Mundialito.Api`
- Frontend: `cd frontend && npm install && npm run dev`
- Todo junto: `docker compose up --build`
