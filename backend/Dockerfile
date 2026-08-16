# Build context is the repository root, so Directory.Build.props and every project are reachable.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore against the project files alone first, so this layer is reused whenever only source changed.
COPY Directory.Build.props ./
COPY backend/src/TestR.Api/TestR.Api.csproj                     backend/src/TestR.Api/
COPY backend/src/TestR.Application/TestR.Application.csproj      backend/src/TestR.Application/
COPY backend/src/TestR.Domain/TestR.Domain.csproj                backend/src/TestR.Domain/
COPY backend/src/TestR.Infrastructure/TestR.Infrastructure.csproj backend/src/TestR.Infrastructure/
RUN dotnet restore backend/src/TestR.Api/TestR.Api.csproj

COPY backend/src/ backend/src/
RUN dotnet publish backend/src/TestR.Api/TestR.Api.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# curl is only here so compose can health-check the container.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

# SQLite file lives on a volume so the data survives a rebuild.
RUN mkdir -p /data
VOLUME /data

ENV ASPNETCORE_URLS=http://+:8080 \
    ConnectionStrings__Default="Data Source=/data/app.db"

EXPOSE 8080

HEALTHCHECK --interval=10s --timeout=3s --start-period=20s --retries=5 \
    CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "TestR.Api.dll"]
