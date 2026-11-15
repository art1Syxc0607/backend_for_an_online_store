# syntax=docker/dockerfile:1

# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Сначала только csproj — этот слой кэшируется, пока не меняются пакеты
COPY Domain/Domain.csproj Domain/
COPY Application/Application.csproj Application/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY WebApi/WebApi.csproj WebApi/
RUN dotnet restore WebApi/WebApi.csproj

# Затем исходники и публикация
COPY Domain/ Domain/
COPY Application/ Application/
COPY Infrastructure/ Infrastructure/
COPY WebApi/ WebApi/
RUN dotnet publish WebApi/WebApi.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

# curl нужен только для HEALTHCHECK (в aspnet-образе его нет)
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Каталоги под загруженные файлы и логи (монтируются томами).
# Владелец — непривилегированный пользователь app из образа .NET.
# Для уже существующих volume'ов, созданных от root, может потребоваться
# однократный chown (см. README).
RUN mkdir -p /app/wwwroot /app/logs \
    && chown -R $APP_UID /app/wwwroot /app/logs

COPY --from=build /app/publish .

USER $APP_UID

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=90s --retries=3 \
    CMD curl -fsS -H "X-Forwarded-Proto: https" http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "WebApi.dll"]
