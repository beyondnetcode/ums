# 📘 Functional Story 12: Ejecutar Proceso de Promoción de Rol

Este documento especifica el flujo automatizado y manual para la evolución de roles de un usuario basándose en criterios de mérito y cumplimiento.

---

## 🏛️ 1. Definición del Caso de Uso

| Atributo | Especificación |
| :--- | :--- |
| **Nombre** | Ejecutar Proceso de Promoción de Rol |
| **Actor Principal** | Sistema (Background Worker) / Administrador (Aprobador) |
| **Precondiciones** | Jerarquía de roles definida y criterios de promoción configurados en `ROLE_PROMOTION_CRITERIA`. |
| **Postcondiciones** | El usuario recibe un nuevo rol en su perfil y se registra la trazabilidad de la promoción. |

---

## 🔄 2. Flujo de Transacción

### A. Flujo Principal
1.  El proceso en segundo plano (Worker) escanea periódicamente a los usuarios para evaluar su elegibilidad para el siguiente nivel jerárquico.
2.  El sistema verifica los flags activos en `ROLE_PROMOTION_CRITERIA` (Antigüedad, Documentos válidos, Scoring).
3.  Si se cumplen todos los criterios automatizados, el sistema cambia el estado del proceso a `CRITERIA_MET` y envía una notificación de "Oportunidad de Promoción" al administrador.
4.  El sistema inicializa un flujo de aprobación (`APPROVAL_REQUEST`).
5.  El administrador revisa la evidencia y aprueba la solicitud.
6.  El sistema actualiza el rol del usuario en la tabla `PROFILE` y marca el proceso como `PROMOTED`.

---

## 🛡️ 3. Flujos Alternativos y Manejo de Excepciones

### Flujo Alternativo A: Criterios no cumplidos
*   Si el usuario no cumple uno o más flags (ej. documento expirado), el sistema mantiene el estado `EVALUATING` y registra el motivo del rechazo automático para consulta del administrador.

### Flujo Alternativo B: Rechazo de Promoción
*   Si el administrador rechaza la promoción (ej. desempeño insuficiente no medible por flags), el proceso vuelve a `EVALUATING` con un periodo de bloqueo configurable para nuevos intentos.

---

## 📋 4. Detalles de Implementación

### Entidades Involucradas
- `ROLE_PROMOTION_CRITERIA`
- `USER_PROMOTION_PROCESS`
- `ROLE`
- `APPROVAL_REQUEST`

### Criterios de Aceptación
1.  La promoción solo puede ocurrir hacia un rol con `HierarchyLevel` superior al actual.
2.  El sistema no debe permitir la promoción si existen documentos mandatorios expirados.
3.  Toda promoción exitosa debe quedar registrada en el historial inmutable con referencia a la aprobación manual.
