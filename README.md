# Mundialito de Fútbol Corporativo

Sistema de gestión de un torneo de fútbol corporativo: equipos, jugadores, partidos, resultados, tabla de posiciones y goleadores.

Prueba técnica con enfoque arquitectónico: Clean Architecture, CQRS, Unit of Work, Result Pattern, REST idempotente, observabilidad y Docker. El enunciado completo está en [docs/ENUNCIADO.md](docs/ENUNCIADO.md).

## Stack

| Capa | Tecnología |
|---|---|
| Backend | .NET 8 (ASP.NET Core Web API) |
| Escritura (Commands) | EF Core + Unit of Work |
| Lectura (Queries) | Dapper |
| Base de datos | SQL Server 2022 |
| Logging / trazas | Serilog (logs estructurados, CorrelationId/TraceId) |
| Frontend | Next.js (App Router, TypeScript) |
| Contenedores | Docker + docker-compose |
| Pruebas | xUnit |

## Estructura

```
backend/
  src/
    Mundialito.Domain          # Entidades, value objects, eventos de dominio
    Mundialito.Application     # Commands, Queries, handlers, interfaces, Result
    Mundialito.Infrastructure  # EF Core, Dapper, UnitOfWork, seed, idempotencia
    Mundialito.Api             # Controllers, middlewares, mapeo Result → HTTP
  tests/
    Mundialito.UnitTests
frontend/                      # Next.js
docs/
  ENUNCIADO.md
  arquitectura/                # Diagrama de arquitectura
  postman/                     # Colección y entorno de Postman
docker-compose.yml
```

## Cómo levantar el proyecto

### Requisitos
- Docker y Docker Compose
- (Opcional, para desarrollo local) .NET 8 SDK y Node.js 20+

### Con Docker

1. Copiar las variables de entorno y ajustar la contraseña:
   ```bash
   cp .env.example .env
   ```
2. Levantar todo:
   ```bash
   docker compose up --build
   ```
3. Servicios:
   - Frontend: http://localhost:3000
   - API: http://localhost:8080
   - SQL Server: `localhost,1433`

Al iniciar, la API aplica las migraciones y carga datos de ejemplo si la base está vacía (4 equipos, 5 jugadores por equipo, 6 partidos, 3 con resultado).

### Desarrollo local

```bash
cd backend
dotnet build
dotnet test
dotnet run --project src/Mundialito.Api
```

```bash
cd frontend
npm install
npm run dev
```

## Pendiente de documentar
- Endpoints de la API
- Diagrama de arquitectura
- Colección de Postman
- Decisiones de diseño
