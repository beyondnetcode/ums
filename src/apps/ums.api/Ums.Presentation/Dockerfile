# =========================================================
# Phase 1: Build Stage
# =========================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Copy shell libraries first (they are referenced by the main projects)
COPY libs/shell/factory/src/Ums.Shell.Factory/Ums.Shell.Factory.csproj libs/shell/factory/src/Ums.Shell.Factory/
COPY libs/shell/factory/src/Ums.Shell.Factory.sln libs/shell/factory/src/
COPY libs/shell/aop/src/Ums.Shell.Aop/Ums.Shell.Aop.csproj libs/shell/aop/src/Ums.Shell.Aop/
COPY libs/shell/aop/src/Ums.Shell.Aop.Aspects/Ums.Shell.Aop.Aspects.csproj libs/shell/aop/src/Ums.Shell.Aop.Aspects/
COPY libs/shell/aop/src/Ums.Shell.Aop.Aspects.Logger/Ums.Shell.Aop.Aspects.Logger.csproj libs/shell/aop/src/Ums.Shell.Aop.Aspects.Logger/
COPY libs/shell/aop/src/Ums.Shell.Aop.Aspects.Logger.Serilog/Ums.Shell.Aop.Aspects.Logger.Serilog.csproj libs/shell/aop/src/Ums.Shell.Aop.Aspects.Logger.Serilog/
COPY libs/shell/aop/src/Ums.Shell.Aop.DispatchProxy/Ums.Shell.Aop.DispatchProxy.csproj libs/shell/aop/src/Ums.Shell.Aop.DispatchProxy/
COPY libs/shell/aop/src/Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer/Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer.csproj libs/shell/aop/src/Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer/
COPY libs/shell/aop/src/Ums.Shell.Aop.sln libs/shell/aop/src/
COPY libs/shell/bootstrapper/src/Ums.Shell.Bootstrapper/Ums.Shell.Bootstrapper.csproj libs/shell/bootstrapper/src/Ums.Shell.Bootstrapper/
COPY libs/shell/bootstrapper/src/Ums.Shell.Bootstrapper.DependencyInjection/Ums.Shell.Bootstrapper.DependencyInjection.csproj libs/shell/bootstrapper/src/Ums.Shell.Bootstrapper.DependencyInjection/
COPY libs/shell/bootstrapper/src/Ums.Shell.Bootstrapper.AutoMapper/Ums.Shell.Bootstrapper.AutoMapper.csproj libs/shell/bootstrapper/src/Ums.Shell.Bootstrapper.AutoMapper/
COPY libs/shell/bootstrapper/src/Ums.Shell.Bootstrapper.Observability/Ums.Shell.Bootstrapper.Observability.csproj libs/shell/bootstrapper/src/Ums.Shell.Bootstrapper.Observability/
COPY libs/shell/bootstrapper/src/Ums.Shell.Bootstrapper.sln libs/shell/bootstrapper/src/
COPY libs/shell/ddd/src/Ums.Shell.Ddd/Ums.Shell.Ddd.csproj libs/shell/ddd/src/Ums.Shell.Ddd/
COPY libs/shell/ddd/src/Ums.Shell.Ddd.ValueObjects/Ums.Shell.Ddd.ValueObjects.csproj libs/shell/ddd/src/Ums.Shell.Ddd.ValueObjects/
COPY libs/shell/ddd/src/Ums.Shell.Ddd/Ums.Shell.Ddd.sln libs/shell/ddd/src/Ums.Shell.Ddd/

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
