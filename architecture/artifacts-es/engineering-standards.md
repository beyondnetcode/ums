# Estándares Globales de Ingeniería y Guías del Desarrollador (Manifiesto BMAD)

## 1.  Principios de Ingeniería Núcleo (Obligatorios)
Todo el código, wrappers y diseños arquitectónicos dentro de este monorepo **DEBEN** adherirse estrictamente a los siguientes principios. Las revisiones de código rechazarán cualquier Pull Request que viole estas bases:

*   **SOLID**: Responsabilidad Única, Abierto/Cerrado, Sustitución de Liskov, Segregación de Interfaces e Inversión de Dependencias.
*   **DRY (Don't Repeat Yourself)**: Eliminar duplicación innecesaria. Consolidar lógica compartida en utilidades o librerías de núcleo compartido.
*   **KISS (Keep It Simple, Stupid)**: Evitar la sobre-ingeniería. Escribir código fácil de leer, entender y depurar.
*   **YAGNI (You Aren't Gonna Need It)**: No añadir funcionalidad, abstracciones o herramientas hasta que sean estrictamente necesarias.
*   **SoC (Separation of Concerns)**: Mantener las capas completamente aisladas. Un controlador no debe escribir lógica de negocio; un caso de uso no debe ejecutar SQL crudo.
*   **Clean Code y Arquitectura Limpia**: Mantener límites estrictos (Adaptadores vs. Núcleo). Asegurar la legibilidad del código y nombres que revelen la intención.
*   **Seguridad por Diseño y OWASP**: Validar todas las entradas (DTOs), sanear salidas, aplicar RBAC de forma nativa y prevenir inyecciones SQL/NoSQL por defecto.

---

## 2.  Diseño Dirigido por el Dominio (DDD): Opcional y Pragmático
Aunque nuestra arquitectura soporta DDD táctico y estratégico:
**El uso de DDD es estrictamente OPCIONAL**. 
Solo debe usarse cuando aporte valor tangible a un dominio de negocio complejo. No debe considerarse una "camisa de fuerza" obligatoria o restrictiva. Para operaciones CRUD simples, los Casos de Uso Hexagonales estándar y los Data Mappers son más que suficientes. Aplicar DDD en exceso a entidades simples se considera un anti-patrón (Sobre-ingeniería).

---

## 3.  Anti-patrones Arquitectónicos y de Código (Estrictamente Prohibidos)
Para garantizar una alta mantenibilidad y baja deuda técnica, las siguientes prácticas están explícitamente prohibidas:
*   **Alto Acoplamiento**: Dependencias directas sobre herramientas de terceros concretas dentro del Núcleo (Viola DIP).
*   **Clases Dios / Módulos Mágicos**: Clases que manejan enrutamiento, validación, lógica de negocio y persistencia simultáneamente.
*   **Vendor Lock-In sin Adaptadores**: Hardcodear SDKs (ej. AWS SDK, Redis) fuera de Puertos/Adaptadores de infraestructura aislados.
*   **Código Espagueti**: Falta de estructura async/await o mónadas funcionales (como el Result Pattern).
*   **Excepciones Ignoradas**: Capturar errores sin registrarlos adecuadamente o retornar errores 500 genéricos sin IDs de traza.

---

## 4.  Gobernanza Técnica y Mecanismos de Aplicación
Confiamos en la **Aplicación Automatizada** para asegurar que estos principios sean sostenibles en el tiempo dentro del marco BMAD:

1.  **Linters y Reglas Arquitectónicas**: `eslint-plugin-boundaries` fallará el build si un desarrollador importa una capa externa (infraestructura) en una capa interna (núcleo).
2.  **Análisis de Código Estático**: `eslint-plugin-sonarjs` detecta complejidad cognitiva y deuda técnica antes del commit.
3.  **Quality Gates en CI/CD**: GitHub Actions bloqueará el merge si las pruebas fallan o el build se rompe.
4.  **Pruebas Automatizadas y Cobertura**: Las pruebas unitarias y E2E son obligatorias. Se requiere una **cobertura de código >70%**.
5.  **Escaneo de Seguridad**: `npm audit` obligatorio en pipelines de CI/CD para bloquear dependencias vulnerables.
6.  **Estándares de Formato**: Prettier y ESLint aplicados vía hooks de `pre-commit` con Husky.

---

## 5.  Matriz de Prioridad de Decisiones
Al tomar una decisión técnica, se deben priorizar los siguientes atributos en este orden:
1.  **Mantenibilidad**
2.  **Escalabilidad**
3.  **Extensibilidad**
4.  **Desacoplamiento**
5.  **Observabilidad**
6.  **Seguridad**
7.  **Resiliencia**
8.  **Testabilidad**
9.  **Rendimiento**
10. **Claridad Arquitectónica**

---

## 6.  Checklist de Calidad para Pull Requests
Antes de enviar un PR, los desarrolladores deben verificar:
- [ ] Ninguna lógica de capa externa se filtra al Dominio.
- [ ] Los intereses transversales (Logging, Caché) usan Decoradores o Puertos.
- [ ] Se usó DDD solo si la complejidad del dominio lo justificaba.
- [ ] La cobertura de pruebas para la nueva característica es >70%.
- [ ] `npm run lint` y `npm run test` pasan localmente.
