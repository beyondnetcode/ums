# UMS Architecture Diagram

## System Context (C4 Level 1)

```mermaid
graph TB
 User[ Admin User]
 Browser[ Web Browser]
 WebApp[ UMS Web App\nReact + Vite]
 API[ UMS API\n.NET 10 + HotChocolate]
 DB[(SQL Server 2022)]

 User -->|HTTPS| Browser
 Browser -->|SPA| WebApp
 WebApp -->|GraphQL Queries| API
 WebApp -->|REST Commands| API
 API -->|EF Core| DB
```

## Container Diagram (C4 Level 2)

```mermaid
graph TB
 subgraph "Browser"
 React[React 18 App]
 Zustand[Zustand Stores]
 TanStack[TanStack Query]
 GraphQL[GraphQL Client]
 Axios[Axios HTTP Client]
 end

 subgraph "UMS Web App (src/apps/ums.web-app)"
 Domain[domain/\nEntities + Schemas]
 Application[application/\nHooks + Stores]
 Infrastructure[infrastructure/\nHTTP + GraphQL]
 Presentation[presentation/\nComponents + Screens]

 Domain --> Application
 Application --> Infrastructure
 Presentation --> Application
 Infrastructure -.->|implements| Application
 end

 subgraph "Backend (src/apps/ums.api)"
 GraphQLServer[HotChocolate\nGraphQL Server]
 RESTServer[Minimal APIs\nREST Server]
 EFCore[EF Core\nData Access]
 end

 React --> Zustand
 React --> TanStack
 TanStack --> GraphQL
 TanStack --> Axios
 GraphQL --> GraphQLServer
 Axios --> RESTServer
 GraphQLServer --> EFCore
 RESTServer --> EFCore
```

## Layer Dependencies (Clean Architecture)

```mermaid
graph LR
 subgraph "Domain (PURE)"
 Entities[Entities]
 Schemas[Zod Schemas]
 VOs[Value Objects]
 end

 subgraph "Application"
 Hooks[React Hooks]
 Stores[Zustand Stores]
 Utils[Utilities]
 end

 subgraph "Infrastructure"
 HTTP[HTTP Client]
 GQL[GraphQL Client]
 CSRF[CSRF Handler]
 end

 subgraph "Presentation"
 Components[Shared Components]
 Screens[Context Screens]
 Layouts[Layouts]
 end

 Presentation --> Application
 Application --> Domain
 Infrastructure -.->|inject| Application
```

## State Management Flow

```mermaid
graph TB
 subgraph "Server State"
 GQLQuery[GraphQL Query]
 RESTCmd[REST Command]
 Cache[(TanStack Query Cache)]
 end

 subgraph "Client State"
 Auth[auth.store]
 Theme[theme.store\npersist]
 I18n[i18n.store]
 Notif[notification.store]
 end

 subgraph "React Components"
 Screen[Screen Component]
 Hook[useEntityList\nuseNotifiedMutation]
 end

 Screen --> Hook
 Hook --> GQLQuery
 Hook --> RESTCmd
 GQLQuery --> Cache
 RESTCmd --> Cache
 Screen --> Auth
 Screen --> Theme
 Screen --> I18n
 Screen --> Notif
 Cache --> Screen
```

## Security Architecture

```mermaid
graph TB
 subgraph "Browser"
 App[React App]
 CSRF[CSRF Token\nCookie + Meta]
 end

 subgraph "Nginx (Production)"
 CSP[CSP Header\nno unsafe-eval]
 HSTS[HSTS Header]
 XFrame[X-Frame-Options: DENY]
 end

 subgraph "Backend"
 DevAuth[DevAuthMiddleware\n(dev only)]
 JWTAuth[JWT Authentication\n(production)]
 CORS[CORS Policy]
 end

 App -->|Request + CSRF| CSRF
 CSRF -->|X-CSRF-Token| Nginx
 Nginx -->|CSP + HSTS + XFrame| App
 Nginx -->|Forward| DevAuth
 DevAuth -->|X-User-Id| App
 JWTAuth -.->|future| DevAuth
```
