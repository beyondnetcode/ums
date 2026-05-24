# BeyondNet.Aop

<p>
  <strong>English</strong> | <a href="README.es.md">Español</a>
</p>

BeyondNet.Aop is a high-performance Aspect-Oriented Programming (AOP) framework for .NET 10. It allows you to cleanly separate cross-cutting concerns (like logging, error handling, retries, etc.) from your core business logic using native `.NET` `DispatchProxy` with heavy caching optimizations.

## 📚 Documentation

The documentation is organized into the following sections:

- 📖 [Glossary](docs/en/glossary.md): Understand the core concepts of AOP used in this framework.
- 🚀 [Getting Started](docs/en/getting-started.md): A step-by-step guide to writing and using your first aspect.
- 🏗️ [Architecture](docs/en/architecture.md): Deep dive into how the interception pipeline and caching mechanisms work.
- 🧩 [Components](docs/en/components.md): Overview of the main packages and internal modules.
- 🛠️ [Tools](docs/en/tools.md): Built-in tools and supported integrations (like Serilog and MS Dependency Injection).

## Quick Start Overview

To use the framework, you generally:
1. Define a target interface and its implementation.
2. Define a class inheriting from `AbstractAspect<T>` containing your cross-cutting logic.
3. Decorate your target interface methods with the aspect attribute.
4. Register the proxy in your DI container using the installer.

For detailed instructions and code examples, please see the [Getting Started](docs/en/getting-started.md) guide.

## Features
- **High Performance**: Uses `ConcurrentDictionary` to cache reflection calls and dynamic expression compilation (O(1) lookups).
- **Clean Code**: Strict naming conventions and semantic exceptions.
- **Native .NET**: Built on top of `System.Reflection.DispatchProxy` without requiring complex post-compilation weaving tools.
- **Extensible**: Easily plug in custom loggers or evaluation engines.
