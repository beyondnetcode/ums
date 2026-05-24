# BeyondNet.Aop

<p>
  <a href="README.en.md">English</a> | <strong>Español</strong>
</p>

BeyondNet.Aop es un framework de Programación Orientada a Aspectos (AOP) de alto rendimiento para .NET 10. Te permite separar limpiamente la lógica transversal (como logging, manejo de errores, reintentos, etc.) de la lógica central de negocio utilizando el `DispatchProxy` nativo de `.NET` combinado con fuertes optimizaciones de caché.

## 📚 Documentación

La documentación está organizada en las siguientes secciones:

- 📖 [Glosario](docs/es/glossary.md): Entiende los conceptos centrales de AOP utilizados en este framework.
- 🚀 [Guía de Inicio](docs/es/getting-started.md): Un tutorial paso a paso para escribir y usar tu primer aspecto.
- 🏗️ [Arquitectura](docs/es/architecture.md): Profundiza en cómo funcionan el flujo de intercepción y los mecanismos de caché.
- 🧩 [Componentes](docs/es/components.md): Descripción general de los paquetes principales y módulos internos.
- 🛠️ [Herramientas](docs/es/tools.md): Herramientas integradas e integraciones soportadas (como Serilog e Inyección de Dependencias de MS).

## Resumen Rápido

Para usar el framework, generalmente debes:
1. Definir una interfaz destino y su implementación.
2. Definir una clase que herede de `AbstractAspect<T>` con tu lógica transversal.
3. Decorar los métodos de tu interfaz con el atributo del aspecto.
4. Registrar el proxy en tu contenedor de Inyección de Dependencias.

Para instrucciones detalladas y ejemplos de código, por favor consulta la [Guía de Inicio](docs/es/getting-started.md).

## Características
- **Alto Rendimiento**: Usa `ConcurrentDictionary` para cachear llamadas de reflexión y compilación dinámica de expresiones (búsquedas en O(1)).
- **Código Limpio**: Convenciones de nombrado estrictas y excepciones semánticas.
- **.NET Nativo**: Construido sobre `System.Reflection.DispatchProxy` sin requerir herramientas complejas de compilación post-procesada.
- **Extensible**: Fácil de conectar a librerías de logging o motores de evaluación personalizados.
