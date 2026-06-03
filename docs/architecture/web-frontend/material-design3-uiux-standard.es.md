# Especificacion de Diseno UI/UX de Material Design 3

Este documento establece los estandares de diseno UI/UX para las aplicaciones web React del Sistema de Gestion de Usuarios (UMS). Estas pautas alinean nuestras implementaciones con los principios de Material Design 3 (M3), garantizando una experiencia de usuario limpia, minimalista, profesional, consistente y altamente accesible.

---

## 1. Tema Visual y Tokens de Diseno Semanticos

La interfaz de la aplicacion esta construida sobre la paleta de colores semanticos de Material Design 3, mapeada a traves de variables CSS basadas en HSL en el sistema de temas. Esto asegura un soporte solido para los modos claro y oscuro mientras se mantienen limites de alto contraste.

### 1.1. Mapeo de la Paleta Base

| Rol de Token de Diseno | Variable CSS | Funcion de Mapeo HSL | Rol en Modo Claro | Rol en Modo Oscuro |
|---|---|---|---|---|
| Primary | `--color-m3-primary` | `hsl(var(--color-m3-primary))` | Acentos de marca para foco | Destacados activos de marca |
| Primary Container | `--color-m3-primary-container` | `hsl(var(--color-m3-primary-container))` | Fondo de listas seleccionadas | Fondos de tarjetas de acento |
| Secondary | `--color-m3-secondary` | `hsl(var(--color-m3-secondary))` | Texto atenuado, descriptores | Elementos de subtitulo |
| Surface | `--color-m3-surface` | `hsl(var(--color-m3-surface))` | Fondos de pantalla | Fondos de paneles/modulos |
| Surface Container | `--color-m3-surface-container` | `hsl(var(--color-m3-surface-container))` | Paneles contenedores principales | Capas internas de cajones |
| Outline | `--color-m3-outline` | `hsl(var(--color-m3-outline))` | Bordes de control de alto contraste | Bordes sutiles de separadores |
| Error | `--color-m3-error` | `hsl(var(--color-m3-error))` | Acciones destructivas, errores | Estado de validacion negativo |

### 1.2. Superposiciones de Estado Interactivo
Todos los elementos interactivos (botones, elementos de fila, elementos de cuadricula) deben implementar retroalimentacion de estado visual utilizando reglas de transicion CSS:
- **Estado Hover:** Aplicar una capa de fondo superpuesta usando `opacity: 0.08` del color de estado o desplazar ligeramente el brillo.
- **Estado Focus:** Mostrar un contorno de enfoque de alto contraste usando `outline: 3px solid hsl(var(--color-m3-primary))` y `outline-offset: 2px` en `focus-visible`.
- **Estado Activo (Presionado):** Aumentar la opacidad de la superposicion de estado a `0.12`.
- **Estado Deshabilitado:** Renderizar controles con `opacity: 0.38` y eliminar los escuchas de interaccion hover/activo.

---

## 2. Cuadricula de Diseno y Escala Espacial

UMS impone un diseno de cuadricula estandarizado basado en puntos de interrupcion responsivos consistentes e incrementos de espaciado para mantener los modulos estructuralmente cohesivos.

### 2.1. Incrementos de Espaciado
Para lograr alineaciones de diseno solidas, todos los rellenos, margenes y espacios de cuadricula deben usar valores derivados de una escala base de 8px:
- **4px (0.25rem):** Microespaciadores (ej. espaciado de etiqueta a entrada).
- **8px (0.5rem):** Elementos compactos (ej. relleno de celda, espacios de lista).
- **12px (0.75rem):** Espaciadores medios (ej. margenes internos de tarjetas, espacios de formulario).
- **16px (1.0rem):** Relleno principal (ej. canaletas de pagina, espacios estandar).
- **24px (1.5rem):** Canaletas espaciosas (ej. separadores de seccion).

### 2.2. Reglas de Proximidad de Gestalt para Formularios
- El espacio vertical entre una etiqueta de campo y su control de entrada asociado debe ser estrictamente menor que el espacio que separa un grupo de formulario del siguiente.
- Valores recomendados para el diseno de formularios:
  - Espaciado de etiqueta a entrada: 4px (0.25rem).
  - Espaciado de entrada a texto de ayuda/error: 4px (0.25rem).
  - Espaciado entre grupos de formularios adyacentes: 16px (1.0rem).

---

## 3. Escala Tipografica

La tipografia debe mantener una consistencia semantica absoluta mediante el mapeo directo a las siguientes escalas de tipo de Material Design 3 utilizando unidades relativas (`rem`):

| Rol de Escala M3 | Equivalente de Clase CSS | Tamano de Fuente | Peso de Fuente | Altura de Linea | Uso de Mayusculas |
|---|---|---|---|---|---|
| Headline Medium | `.text-m3-headline-medium` | 1.75rem (28px) | Semi-Bold (600) | 1.3 | Estandar |
| Title Large | `.text-m3-title-large` | 1.375rem (22px) | Medium (500) | 1.3 | Estandar |
| Title Medium | `.text-m3-title-medium` | 1.0rem (16px) | Medium (500) | 1.4 | Estandar |
| Body Medium | `.text-m3-body-medium` | 0.875rem (14px) | Regular (400) | 1.5 | Estandar |
| Label Large | `.text-m3-label-large` | 0.875rem (14px) | Medium (500) | 1.4 | Oracion |
| Label Small | `.text-m3-label-small` | 0.6875rem (11px) | Medium (500) | 1.5 | Mayusculas |

---

## 4. Estandares de Componentes de UI Comunes

Todos los modulos de UMS deben utilizar estos componentes de presentacion estandarizados en lugar de implementar variaciones de estilo ad-hoc.

### 4.1. Carcasa de Tablero de Pagina (`PageDashboardShell` y `MasterDetailLayout`)
- **Estructura:** Encapsula todo el espacio del viewport. Divide el diseno en una zona de navegacion, listado principal (master) y un panel de inspector de detalles (detail).
- **Elemento Splitter:** La barra divisoria debe tener una descripcion accesible (`splitterLabel="Resize panel"`). Debe admitir un cambio de tamano suave al arrastrar el mouse y colapsar sin problemas.

### 4.2. Botones de Accion (`M3Button`)
- **Variante Filled:** Para acciones primarias en una pantalla (ej. "Registrar inquilino", "Guardar cambios"). Maximo un boton lleno primario por panel de tablero.
- **Variante Outlined:** Para acciones secundarias (ej. "Cancelar", "Editar perfil", "Restablecer filtros").
- **Variante Text:** Para opciones de fila en linea o claves de cancelacion modal.
- **Clase Destructiva:** Las acciones de alto riesgo (ej. "Desactivar", "Bloquear") deben usar colores de error HSL en los estados flotante y presionado.

### 4.3. Entradas de Texto (`M3TextField`)
- **Estructura Semantica:** Envuelto en `<label>` o vinculado explicitamente usando parametros `for` e `id` para ayudar a los lectores de asistencia.
- **Configuracion de Autofill:** Los autocompletados deben mapearse explicitamente para maximizar la experiencia del usuario:
  - Campos de correo electronico → `autocomplete="username"` o `autocomplete="email"`.
  - Campos de contrasena → `autocomplete="current-password"` (inicio de sesion) o `autocomplete="new-password"` (creacion).
  - Entradas de texto normales → `autocomplete="off"` o roles contextuales explicitos.
- **Contornos Interactivos:** Los elementos de borde deben tener un alto contraste visual contra los paneles de superficie (relacion de contraste minima de 4.5:1).

### 4.4. Dialogos (`M3Dialog` y `ConfirmDialog`)
- **Superposicion de Scrim:** Fondo de superficie oscurecido (`opacity: 0.32` a `0.5`) con efecto de desenfoque (`backdrop-filter: blur(4px)`) para mantener el enfoque centrado.
- **Cierre Modal:** El clic en el scrim debe activar de manera segura las devoluciones de llamada de desestimacion, a menos que este activa una transaccion forzada.

---

## 5. Restricciones de Formulario Estandar y Temporizacion de Validacion

Los elementos de formulario deben implementar restricciones de validacion no intrusivas, evitando advertencias de error prematuras mientras el usuario escribe:

| Fase de Interaccion | Evento de Validacion | Comportamiento Esperado | Proposito |
|---|---|---|---|
| **Escritura activa** | `input` | Limpiar los estados de error existentes de inmediato. | Evitar bloquear o distraer al usuario. |
| **Salir del control** | `blur` / `focusout` | Realizar comprobaciones de validacion y renderizar errores. | Validacion contextual una vez que el usuario termina. |
| **Envio de formulario** | `submit` | Comprobar todas las entradas, bloquear carga util, dirigir enfoque. | Guardian: interceptar carga util incorrecta. |

---

## 6. Lista de Verificacion de Responsividad y Accesibilidad

Todas las pantallas React de frontend deben pasar esta lista de verificacion automatizada y manual antes de la implementacion:

- `[ ]` **Objetivos de Toque Minimos:** Todos los botones y entradas interactivas deben tener una altura minima de clic de `48px` en disenos moviles.
- `[ ]` **Optimizacion de Autofill:** Las entradas utilizan configuraciones correctas de `inputmode` (ej. `inputmode="numeric"` para codigos de digitos).
- `[ ]` **Navegacion por Teclado:** Los bucles de tabulacion fluyen secuencialmente. Los modales capturan el enfoque hasta que se cierran.
- `[ ]` **Contraste Visual:** Todo el contenido de texto satisface una relacion de contraste minima de 4.5:1 contra los paneles de fondo.
- `[ ]` **Indicadores de Enfoque:** Los estados focus-visible activan bordes solidos de alto contraste sin contornos ocultos.

---

## 7. Notas Arquitectónicas de CQRS y Pendientes (TODOs)

> [!IMPORTANT]
> **Estrategia de transporte REST vs GraphQL:**
> El frontend usa un modelo de transporte mixto. Algunos contextos delimitados son intencionalmente solo REST, mientras que otros siguen usando GraphQL para lecturas donde esa sigue siendo la implementación activa.
> - Las lecturas de configuración que hoy son solo REST se manejan mediante `httpClient`.
> - Los contextos delimitados que siguen respaldados por GraphQL permanecen activos en áreas como autorización e identidad.
> - Cualquier migración futura de regreso a GraphQL debe evaluarse por contexto delimitado, no como una regla global para todo el frontend.
