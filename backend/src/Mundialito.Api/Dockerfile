# Contexto de build: carpeta backend/ (ver docker-compose.yml)

# ---------- Etapa 1: compilar y publicar ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Primero solo los .csproj: si no cambian, Docker reutiliza la capa del restore.
COPY src/Mundialito.Domain/Mundialito.Domain.csproj src/Mundialito.Domain/
COPY src/Mundialito.Application/Mundialito.Application.csproj src/Mundialito.Application/
COPY src/Mundialito.Infrastructure/Mundialito.Infrastructure.csproj src/Mundialito.Infrastructure/
COPY src/Mundialito.Api/Mundialito.Api.csproj src/Mundialito.Api/
RUN dotnet restore src/Mundialito.Api/Mundialito.Api.csproj

COPY src/ src/
RUN dotnet publish src/Mundialito.Api/Mundialito.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ---------- Etapa 2: imagen final (solo runtime) ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

# Usuario sin privilegios incluido en las imágenes oficiales de .NET 8
USER $APP_UID

ENTRYPOINT ["dotnet", "Mundialito.Api.dll"]
