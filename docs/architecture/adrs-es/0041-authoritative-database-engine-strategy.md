# ADR 0041: Estrategia de Motor de Base de Datos Autorizada por Stack de Lenguaje

## Estado
Propuestao

## Contexto
A medida que el proyecto UMS evoluciona hacia un monorepo de grado empresarial y migra componentes principales a .NET 8, es necesario estandarizar las preferencias del motor de base de datos para optimizar la alineación del ecosistema, el rendimiento y la mantenibilidad a través de diferentes stacks de lenguaje.

Anteriormente, se utilizaba PostgreSQL como el motor relacional por defecto tanto para prototipos de Node.js como de .NET. Sin embargo, los estándares corporativos para la excelencia en .NET priorizan SQL Server para aplicaciones .NET de misión crítica.

## Decisión
Adoptaremos una estrategia de base de datos consciente del lenguaje (polyglot-aware) basada en el lenguaje principal del servicio de aplicación:

1.  **Para Aplicaciones .NET 8+ (ej., UMS Core API):**
    *   **Motor Relacional:** **SQL Server 2022** (Última versión estable).
    *   **ORM:** Entity Framework Core (EF Core) con `Microsoft.EntityFrameworkCore.SqlServer`.
    *   **Racional:** Integración fluida con el ecosistema .NET, soporte superior de herramientas (SSMS/Azure Data Studio) y planes de ejecución optimizados para cargas de trabajo basadas en MediatR.

2.  **Para Aplicaciones Node.js / NestJS:**
    *   **Motor Relacional:** **PostgreSQL 16**.
    *   **Motor NoSQL:** **MongoDB**.
    *   **Racional:** Madurez de la comunidad, soporte nativo de JSONB en PG y alta velocidad de desarrollo en el ecosistema Node.js.

### Implementación de Seguridad
*   **Seguridad a Nivel de Fila (RLS):** Para los servicios .NET/SQL Server, RLS se implementará utilizando **Políticas de Seguridad** de SQL Server y **Funciones con Valores de Tabla en Línea (iTVF)** para el filtrado, en lugar de las políticas de PostgreSQL.
*   **Multi-Tenancy:** El modelo de "Base de Datos Compartida, Esquema Compartido" sigue siendo obligatorio, aplicado mediante RLS de SQL Server.

## Consecuencias
*   El plan de migración de UMS de NestJS (Node) a .NET 8 debe actualizarse para reemplazar Npgsql con el proveedor de SQL Server.
*   Los entornos de desarrollo local (Docker Compose) incluirán un contenedor `mssql-server-linux` para la API .NET.
*   Los planos arquitectónicos e inventarios técnicos deben actualizarse para reflejar SQL Server como el motor autorizado para el stack .NET.
*   Los sistemas satélite que hereden de está arquitectura de referencia deben seguir estáas mismas preferencias de motor basadas en su stack de lenguaje.
