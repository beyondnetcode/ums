# Functional Story 14: Delegar Gestión de Usuarios entre Administradores

## 1. Propósito de Negocio

Las organizaciones necesitan delegar gestión de usuarios sin entregar poder administrativo irrestricto. UMS debe permitir asignar responsabilidad limitada de gestión preservando propiedad, alcance y revocabilidad.

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **Administrador Delegante** | Otorga un alcance limitado de gestión a otro administrador. |
| **Administrador Receptor** | Gestiona solo los usuarios y alcance delegados. |
| **Administrador Superior** | Puede supervisar o revocar la autoridad delegada. | ## 3. Precondiciones de Negocio

- Ambos administradores pertenecen a un contexto de tenant autorizado.
- El administrador delegante posee o controla los usuarios y el alcance delegado.
- El administrador receptor es elegible para gestionar usuarios.

## 4. Flujo Funcional Principal

1. El administrador delegante selecciona los usuarios a delegar.
2. El administrador delegante selecciona el administrador receptor.
3. El administrador delegante define el alcance y límite temporal opcional.
4. El sistema valida que la delegación no exceda la autoridad propia del delegante.
5. El sistema activa la delegación.
6. El administrador receptor puede gestionar los usuarios delegados solo dentro del alcance aprobado.

## 5. Flujos Alternativos y Excepciones

### A. Alcance No Poseído

Si el administrador delegante intenta delegar autoridad que no posee, el sistema bloquea la operación.

### B. Delegación Circular o Insegura

Si la delegación crea una cadena circular de gestión o escalamiento de privilegios, el sistema la rechaza.

## 6. Reglas de Negocio

1. Ningún administrador puede delegar autoridad que no tiene.
2. La delegación debe limitarse por usuarios, system/suite, tenant o tiempo cuando aplique.
3. La delegación debe ser revocable.
4. Varios administradores pueden gestionar el mismo usuario solo cuando exista delegación explícita.

## 7. Criterios de Aceptación

1. La autoridad delegada queda limitada a usuarios y alcance aprobados.
2. La delegación puede revocarse por el delegante o un administrador superior.
3. Se previene delegación circular.
4. Todo cambio de delegación queda auditable.

## 8. Requisitos Técnicos

> [!WARNING]
> **ESTADO DE IMPLEMENTACIÓN: DIFERIDO / FUERA DE ALCANCE**  
> En la fase actual, la lógica transaccional de delegación administrativa de usuarios (`UserManagementDelegation`) está **diferida** y no está implementada dentro del proyecto principal de dominio de C#.

- Hacer cumplir la lógica de validación recursiva de alcance a nivel de aplicación.
- Soportar el rastreo de auditoría en eventos del ciclo de vida de la delegación.

## 9. Trazabilidad

- Entidades: `UserManagementDelegation` (AR Diferido), `UserAccount` (AR)
- ADRs: ADR-0038, ADR-0044
- Historias relacionadas: FS-10, FS-12
