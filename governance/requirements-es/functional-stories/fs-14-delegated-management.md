# 📘 Functional Story 14: Delegar Gestión de Usuarios entre Administradores

Este documento especifica el flujo para que un administrador delegue la gestión de un pool de usuarios a otro administrador, permitiendo una administración descentralizada y segura.

---

## 🏛️ 1. Definición del Caso de Uso

| Atributo | Especificación |
| :--- | :--- |
| **Nombre** | Delegar Gestión de Usuarios entre Administradores |
| **Actor Principal** | Administrador Delegante |
| **Precondiciones** | Ambos usuarios poseen roles administrativos dentro del mismo Tenant. |
| **Postcondiciones** | Se crea una relación de gestión en la tabla `USER_MANAGEMENT_DELEGATION`. |

---

## 🔄 2. Flujo de Transacción

### A. Flujo Principal
1.  El Administrador Delegante selecciona a uno o más usuarios bajo su cargo.
2.  Selecciona al "Administrador Hijo" que recibirá la facultad de gestión.
3.  Define el alcance de la delegación (ej. restringido a un Sistema/Suite específico).
4.  El sistema valida que el administrador receptor no tenga un nivel jerárquico que cree un conflicto de autoridad circular.
5.  El sistema registra la delegación en `USER_MANAGEMENT_DELEGATION`.
6.  A partir de este momento, el administrador receptor puede ver y gestionar los perfiles de los usuarios delegados dentro del alcance definido.

---

## 🛡️ 3. Flujos Alternativos y Manejo de Excepciones

### Flujo Alternativo A: Alcance no poseído
*   Si el Administrador Delegante intenta delegar permisos sobre un sistema que él mismo no administra, el sistema bloquea la operación bajo el principio de "Nadie delega lo que no posee".

---

## 📋 4. Detalles de Implementación

### Entidades Involucradas
- `USER_MANAGEMENT_DELEGATION`
- `USER_ACCOUNT`
- `SYSTEM_SUITE`

### Criterios de Aceptación
1.  El administrador receptor debe poder realizar acciones (reset de clave, asignación de perfiles) solo sobre los usuarios delegados.
2.  La delegación debe poder revocarse en cualquier momento por el administrador delegante o un administrador superior.
3.  El sistema debe soportar delegación múltiple (un usuario gestionado por más de un administrador).
