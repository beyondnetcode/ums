# Implementaciones AOP de la API

Este documento detalla las implementaciones concretas de Programación Orientada a Aspectos (AOP) utilizadas en la API de UMS. Estos aspectos se construyen sobre la librería `Ums.Shell.Aop` y sirven para desacoplar las preocupaciones transversales de la lógica de negocio central (Comandos y Consultas) en la capa de Aplicación.

---

## 1. LoggerAspect

### Objetivo
Registrar automáticamente la entrada, el éxito, el fallo y la duración de la ejecución de los manejadores (Handlers) de la capa de Aplicación.

### Razón de Ser
Inyectar manualmente `ILogger<T>` en cada manejador de MediatR añade código repetitivo (boilerplate) y conduce a formatos de registro inconsistentes. Al usar un aspecto, garantizamos que todas las operaciones se tracen de manera uniforme, facilitando el análisis de logs en observatorios centralizados como Datadog o ELK.

### Cómo Implementarlo
Aplica el atributo `[LoggerAspect]` a cualquier manejador de comando o consulta.

### Ejemplo de Código
```csharp
using Ums.Application.Common.Aop;

[LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
public class CreateUserAccountCommandHandler : IRequestHandler<CreateUserAccountCommand, Result>
{
    public async Task<Result> Handle(CreateUserAccountCommand request, CancellationToken cancellationToken)
    {
        // La lógica de negocio se ejecuta aquí. El registro (logging) se maneja automáticamente.
        return Result.Success();
    }
}
```

---

## 2. AuditTrailAspect

### Objetivo
Persistir registros inmutables de mutaciones críticas de estado para cumplir con los estándares corporativos de cumplimiento (compliance) y seguridad.

### Razón de Ser
Las pistas de auditoría nunca deben ser omitidas por los desarrolladores. Manejarlas vía AOP asegura que toda operación que muta estado (ej. Crear, Actualizar, Eliminar) resuelva automáticamente las entidades afectadas, el actor, y el resultado (Éxito/Fallo), y lo escriba en el Sink de Auditoría sin contaminar la lógica del dominio.

### Cómo Implementarlo
El `AuditTrailAspect` se registra globalmente en la capa de Infraestructura usando `AddAop()`. Intercepta automáticamente los manejadores decorados con `[AuditTrailAttribute]`.

### Ejemplo de Código
```csharp
using Ums.Application.Common.Aop;

[AuditTrailAttribute(EventType = "UserCreation", WhatChanged = "Created a new user account")]
public class CreateUserAccountCommandHandler : IRequestHandler<CreateUserAccountCommand, Result>
{
    public async Task<Result> Handle(CreateUserAccountCommand request, CancellationToken cancellationToken)
    {
        // El registro de auditoría se emite de forma transparente al finalizar con éxito
        return Result.Success();
    }
}
```

---

## 3. TransactionAspect

### Objetivo
Envolver las operaciones de base de datos dentro de un Unit of Work / Ámbito de Transacción (Transaction Scope) robusto, garantizando confirmaciones atómicas (commits) y reversiones automáticas (rollbacks) en caso de fallo.

### Razón de Ser
Las transacciones de base de datos son una preocupación de infraestructura. Obligar a la capa de aplicación a manejar manualmente `IUnitOfWorkScope.BeginAsync()`, `CommitAsync()` y `RollbackAsync()` satura el código y genera el riesgo de transacciones colgadas si las excepciones no se atrapan correctamente.

### Cómo Implementarlo
Aplica el atributo `[TransactionAspect]` a los manejadores de comandos que realicen escrituras en la base de datos.

### Ejemplo de Código
```csharp
using Ums.Application.Common.Aop;

[TransactionAspect]
public class UpdateSystemSuiteCommandHandler : IRequestHandler<UpdateSystemSuiteCommand, Result>
{
    public async Task<Result> Handle(UpdateSystemSuiteCommand request, CancellationToken cancellationToken)
    {
        // Si este método lanza una excepción, o si retorna Result.Failure, la transacción hace rollback.
        // Si tiene éxito, la transacción se confirma (commit) automáticamente.
        await _repository.UpdateAsync(suite);
        return Result.Success();
    }
}
```

---

## 4. TenantValidationAspect

### Objetivo
Hacer cumplir los límites de multi-tenencia en los bordes más externos de la capa de Aplicación antes de que se ejecute la lógica de dominio.

### Razón de Ser
Depender únicamente de SQL Server RLS (Seguridad a Nivel de Fila) es insuficiente como defensa primaria (Regla 7). La aplicación debe validar proactivamente que el `TenantId` del payload entrante coincide con el `TenantId` de la sesión autenticada (desde `IUserContext`), bloqueando las solicitudes maliciosas entre inquilinos instantáneamente.

### Cómo Implementarlo
Aplica el atributo `[TenantValidationAspect]` a los manejadores que procesan datos limitados al contexto del inquilino (tenant-scoped). El aspecto intercepta el payload, extrae el `TenantId` usando reflexión, y lo verifica contra `IUserContext`.

### Ejemplo de Código
```csharp
using Ums.Application.Common.Aop;

[TenantValidationAspect]
public class GetUserAccountByIdQueryHandler : IRequestHandler<GetUserAccountByIdQuery, Result<UserAccountDto>>
{
    public async Task<Result<UserAccountDto>> Handle(GetUserAccountByIdQuery request, CancellationToken cancellationToken)
    {
        // El aspecto asegura que request.TenantId == _userContext.TenantId.
        // Si difieren, se lanza un UnauthorizedAccessException antes de llegar aquí.
        var user = await _repository.GetByIdAsync(request.Id);
        return Result<UserAccountDto>.Success(user);
    }
}
```
