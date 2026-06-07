# =========================================================
# Phase 1: Build Stage
# =========================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Copy main application projects
COPY apps/ums.api/Ums.Presentation/Ums.Presentation.csproj apps/ums.api/Ums.Presentation/
COPY apps/ums.api/Ums.Application/Ums.Application.csproj apps/ums.api/Ums.Application/
COPY apps/ums.api/Ums.Infrastructure/Ums.Infrastructure.csproj apps/ums.api/Ums.Infrastructure/
COPY apps/ums.api/Ums.Domain/Ums.Domain.csproj apps/ums.api/Ums.Domain/
COPY apps/ums.api/Ums.Globalization/Ums.Globalization.csproj apps/ums.api/Ums.Globalization/

# Restore dependencies
RUN dotnet restore apps/ums.api/Ums.Presentation/Ums.Presentation.csproj

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
