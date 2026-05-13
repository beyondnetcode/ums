# UMS — Sistema de Gestión de Usuarios Empresarial

**Monolito Modular** de grado empresarial diseñado para autorización e identidad de alta escala. Construido bajo **Clean Architecture**, **DDD (Domain-Driven Design)** y patrones **Hexagonales**.

---

## 🧭 Navegación del Proyecto
Accede a las diferentes capas del ciclo de vida del producto:

### 📋 1. Gobernanza y Requisitos
- **[Visión y Contexto](./governance/product/business-context.md)**: Declaración del problema y resumen de la solución.
- **[Roadmap del Producto](./governance/roadmap/versioning-and-audit-strategy.md)**: Estrategia de versionado y logs de auditoría.
- **[Requisitos de Dominio](./governance/requirements/glossary.md)**: Lenguaje ubicuo y modelos conceptuales.

### 🏗️ 2. Ingeniería y Arquitectura
- **[Planos de Arquitectura](./architecture/blueprints-es/architecture-spec.md)**: Modelos C4 y especificaciones técnicas.
- **[Registro de ADRs](./architecture/adrs-es/)**: Registro histórico de decisiones arquitectónicas.
- **[Estándares de Ingeniería](./architecture/artifacts-es/engineering-standards.md)**: Gates de calidad y estándares de código.

### 🛠️ 3. Infraestructura y Operaciones
- **[DevOps e IaC](./infrastructure/)**: Docker, Kong Gateway y configs de K8s.
- **[Observabilidad](./operations/)**: Monitoreo, Tracing (Tempo/OTel) y Dashboards.

### 💻 4. Implementación (Código Fuente)
- **[Portal Frontend](./src/apps/app-web/)**: Aplicación React 18 + Vite.
- **[API Backend](./src/apps/app-api-dotnet/)**: API Core .NET 8 LTS.
- **[Librerías Compartidas](./src/libs/)**: Librerías de dominio y técnicas internas.

---

### 🏛️ ADN Técnico
- **Patrón**: Monolito Modular/Progresivo con Contextos Delimitados estrictos.
- **Seguridad**: Seguridad a Nivel de Fila (RLS) + OAuth2/OIDC.
- **Metodología**: **BMAD-METHOD** para trazabilidad documental asistida por IA.

### 🚦 Inicio Rápido
```powershell
# Entrar al Engine Room e Iniciar
cd src
npm install; npx nx run app-web:dev
```

---
La navegación detallada está disponible en el [**Indice Maestro**](./MASTER_INDEX.es.md).
