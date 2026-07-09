# ──────────────────────────────────────────────────────────────────────────
# Evolith UMS API (.NET 10) — multi-stage build.
#
# BUILD CONTEXT: the ums monorepo `src/` directory. The project graph reaches
# OUTSIDE apps/ums.api (Ums.ReadModels and libs/sdk/dotnet/Ums.Sdk.Contracts),
# so the context must be `src/`, not apps/ums.api.
#   docker build -f apps/ums.api/Dockerfile -t evolith-ums-api:local src/
#
# Entry point: apps/ums.api/Ums.Presentation. Kestrel on :8080 (aspnet default).
# ──────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# The cross-project reference graph (Ums.ReadModels + Ums.Sdk.Contracts under
# libs/) makes a per-csproj restore copy brittle; copy the whole source context
# (bin/obj/node_modules excluded via .dockerignore) and restore/publish the API.
COPY . .

RUN dotnet restore apps/ums.api/Ums.Presentation/Ums.Presentation.csproj
RUN dotnet publish apps/ums.api/Ums.Presentation/Ums.Presentation.csproj \
      -c Release -o /app/publish /p:UseAppHost=false

# ── Runtime image ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

LABEL org.opencontainers.image.title="Evolith UMS API"
LABEL org.opencontainers.image.source="https://github.com/beyondnetcode/ums"

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish ./

# Non-root runtime user (uid 1001 mirrors the Core/Tracker images).
RUN groupadd -g 1001 evolith \
    && useradd -u 1001 -g evolith -M -s /usr/sbin/nologin evolith \
    && chown -R evolith:evolith /app
USER 1001

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

EXPOSE 8080

ENTRYPOINT ["dotnet", "Ums.Presentation.dll"]
