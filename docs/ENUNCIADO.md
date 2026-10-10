# Proyecto: Sistema de Gestión – Mundialito de Fútbol Corporativo

- Tiempo estimado: 48–72 horas
- Modalidad: Prueba asíncrona - Repositorio público en GitHub

## 1. Objetivo

Desarrollar una solución completa con enfoque arquitectónico profesional aplicando:

- Clean Architecture
- CQRS
- EF Core para escritura
- Dapper para lectura
- Unit of Work
- Result Pattern
- REST completo e idempotente
- Trazabilidad y observabilidad
- Frontend desacoplado
- Dockerización

## 2. Arquitectura Obligatoria

Clean Architecture. Capas mínimas:

- Domain
- Application
- Infrastructure
- Api

Reglas:

- Domain no depende de ninguna otra capa
- Application depende solo de Domain
- Infrastructure implementa contratos
- Api solo orquesta

## 3. CQRS (OBLIGATORIO)

Separación clara entre:

**Commands (Write Side)**
- Implementados con EF Core
- Uso obligatorio de Unit of Work
- Transacciones correctamente manejadas

**Queries (Read Side)**
- Implementadas exclusivamente con Dapper
- Consultas optimizadas
- Sin uso de EF para lectura
- Paginación eficiente desde base de datos

## 4. Unit of Work (OBLIGATORIO)

Debe implementarse un patrón Unit of Work que:

- Centralice el manejo de transacciones
- Permita commit explícito
- Garantice consistencia en múltiples repositorios
- Sea usado en todos los Commands

No se permite guardar cambios directamente desde el DbContext fuera del UnitOfWork.

## 5. Result Pattern (OBLIGATORIO)

No se permite usar excepciones como control de flujo.

Debe implementarse `Result` y `Result<T>` con:

- Success
- Failure
- Error Message
- Error Code opcional

La API debe mapear correctamente:

- 200 OK
- 201 Created
- 400 BadRequest
- 404 NotFound
- 409 Conflict

## 6. REST Completo + Idempotencia (OBLIGATORIO)

Uso correcto de verbos HTTP:

- GET → lectura
- POST → creación
- PUT → reemplazo completo (idempotente)
- PATCH → modificación parcial
- DELETE → eliminación idempotente

Idempotencia en POST. Debe implementarse:

- Header `Idempotency-Key`
- Persistencia de la key
- Evitar duplicación de registros
- Retornar la misma respuesta original ante repetición

Se evaluará manejo correcto de conflictos y estados HTTP.

## 7. Observabilidad y Trazabilidad (OBLIGATORIO)

La solución debe incluir:

**Logging estructurado**
- Uso de ILogger o Serilog
- Logs estructurados (no solo strings planos)
- No loggear datos sensibles
- Niveles correctos: Information, Warning, Error, Debug

**Trazabilidad (Tracing)**
- CorrelationId por request
- Middleware que agregue TraceId
- Registro de duración de requests
- Logs enlazados al TraceId

Opcional valorable:
- OpenTelemetry
- Exportación a Jaeger, Zipkin o similar

**Eventos de Dominio (Trazabilidad)**

Al menos:
- Evento cuando se registra resultado
- Evento cuando se crea equipo

Los eventos deben:
- Ser registrados
- Ser loggeados
- Mantener trazabilidad

No es obligatorio Event Bus externo, pero sí buena modelación.

## 8. Requisitos Funcionales

**Equipos**
- CRUD completo
- Filtros
- Paginación
- Ordenamiento dinámico

**Jugadores**
- Registro por equipo
- Filtros
- Paginación

**Partidos**
- Creación
- Registro de resultado
- Filtros por fecha, equipo y estado
- Paginación

**Lógica del Torneo**
- Tabla automática
- Orden por: Puntos, Diferencia de gol, Goles a favor
- Goleadores
- Consultas optimizadas con Dapper

## 9. Seed de Datos (OBLIGATORIO)

Automático al iniciar si DB está vacía:

- 4 equipos
- 5 jugadores por equipo
- 6 partidos
- 3 con resultados

Debe:
- Estar en Infrastructure
- Funcionar con Docker
- Permitir probar tabla inmediatamente

## 10. Filtros y Paginación (OBLIGATORIO)

Todos los GET deben soportar:

- pageNumber
- pageSize
- sortBy
- sortDirection
- filtros por entidad

Respuesta:

```json
{
  "data": [],
  "pageNumber": 1,
  "pageSize": 10,
  "totalRecords": 50,
  "totalPages": 5
}
```

No se permite traer todo a memoria.

## 11. Frontend (Next.js Obligatorio)

Debe incluir:

- Visualización equipos
- Visualización partidos
- Registro resultados
- Tabla posiciones
- Goleadores
- Filtros + paginación
- Manejo correcto de errores

Arquitectura limpia y desacoplada.

## 12. Dockerización (OBLIGATORIO)

Debe incluir:

- Dockerfile API
- Dockerfile Frontend
- docker-compose.yml con: API, DB, Frontend

Debe levantarse con:

```
docker-compose up
```

## 13. Diagrama de Arquitectura (OBLIGATORIO)

Debe representar:

- Capas
- CQRS
- EF Core (write)
- Dapper (read)
- UnitOfWork
- Idempotency storage
- Eventos
- Logs
- Trace flow
- Frontend
- Docker

## 14. Colección de Postman (OBLIGATORIO)

Debe incluir:

- Endpoints organizados
- Casos de: Idempotencia, Filtros, Paginación, Errores
- Variables de entorno

## 15. Pruebas Unitarias (OBLIGATORIO)

Mínimo:

- Tabla de posiciones
- Cálculo de puntos
- Diferencia de gol
- Result Pattern
- Idempotencia
- UnitOfWork commit behavior

Valorable:

- Tests de integración
