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
    ASPNETCORE_ENVIRONMENT=Production

# Каталоги под загруженные файлы и логи (монтируются томами)
RUN mkdir -p /app/wwwroot /app/logs

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "WebApi.dll"]
