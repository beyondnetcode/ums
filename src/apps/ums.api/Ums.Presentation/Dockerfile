# =========================================================
# Phase 1: Build Stage
# =========================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Copy main application projects
COPY src/apps/ums.api/Ums.Presentation/Ums.Presentation.csproj src/apps/ums.api/Ums.Presentation/
COPY src/apps/ums.api/Ums.Application/Ums.Application.csproj src/apps/ums.api/Ums.Application/
COPY src/apps/ums.api/Ums.Infrastructure/Ums.Infrastructure.csproj src/apps/ums.api/Ums.Infrastructure/
COPY src/apps/ums.api/Ums.Domain/Ums.Domain.csproj src/apps/ums.api/Ums.Domain/
COPY src/apps/ums.api/Ums.Globalization/Ums.Globalization.csproj src/apps/ums.api/Ums.Globalization/

# Restore dependencies
RUN dotnet restore src/apps/ums.api/Ums.Presentation/Ums.Presentation.csproj

# Copy everything else
COPY . .

# Build and publish
WORKDIR /src/apps/ums.api/Ums.Presentation
RUN dotnet publish -c Release -o /app/publish

# =========================================================
# Phase 2: Runtime Stage
# =========================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Ums.Presentation.dll"]
