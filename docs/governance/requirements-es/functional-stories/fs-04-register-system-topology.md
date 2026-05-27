# Functional Story 4: Registrar Sistema y Definir Topología de Menú

## 1. Propósito de Negocio

UMS debe permitir que los administradores registren sistemas cliente y describan su estructura de navegación para asignar permisos sobre capacidades reales del negocio.

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **Administrador de Seguridad Global** | Registra sistemas cliente y define su topología de menú. |
| **Dueño del Sistema Cliente** | Proporciona la estructura del sistema y sus acciones de acceso. | ## 3. Precondiciones de Negocio

- El administrador esta autorizado para registrar sistemas.
- El dueño del sistema ha proporcionado módulos, menús, opciones y acciones esperadas.

## 4. Flujo Funcional Principal

1. El administrador inicia el registro de un nuevo sistema cliente.
2. El administrador ingresa nombre, código de negocio e información de enrutamiento.
3. El sistema queda registrado y disponible para configurar su topología.
4. El administrador define módulos, menús, opciones y acciones.
5. El sistema valida que la topologia esta suficientemente completa para soportar plantillas de autorizacion.
6. La topología queda disponible para asignación de permisos y diagnóstico.

## 5. Flujos Alternativos y Excepciones

### A. Código de Sistema Duplicado

Si otro sistema ya usa el mismo código de negocio, UMS impide el registro y solicita elegir un código único.

### B. Topología Incompleta

Si un nodo de topologia esta incompleto, UMS puede guardarlo como borrador pero impide usarlo en plantillas hasta definir las acciones requeridas.

## 6. Reglas de Negocio

1. Los códigos de sistema deben ser únicos.
2. Menús y opciones deben pertenecer a una jerarquía de sistema registrada.
3. Las acciones deben ser explícitas antes de asignarse mediante plantillas.
4. Una topología en borrador no debe otorgar permisos.

## 7. Criterios de Aceptación

1. Un administrador puede registrar un sistema nuevo con código único.
2. Los códigos duplicados son rechazados.
3. La topología de menú puede construirse y revisarse.
4. La topología incompleta no puede usarse para asignación de autorización.

## 8. Requisitos Técnicos

> [!WARNING]
> **ESTADO DE IMPLEMENTACION: ACTIVO**
> `SystemSuite` y su topologia de menus estan implementados en el dominio de Autorizacion. El mantenimiento del catalogo de roles asociado a la suite seleccionada esta cubierto por FS-17.

- Asegurar la persistencia del identificador y metadatos del sistema.
- Asegurar la unicidad de los códigos de sistema.
- Emitir eventos de dominio cuando se registran metadatos de sistema.

## 9. Trazabilidad

- Entidades: `SystemSuite`, `Module`, `Menu`, `SubMenu`, `Option`, `Action`
- ADRs: ADR-0032, ADR-0034, ADR-0047
- Historias relacionadas: FS-02, FS-07, FS-17
