# Guía de Configuración de Kong Gateway (Tier 1)

Para integrar **Kong Open Source** frente a tu **NestJS BFF (Tier 2)**, la mejor práctica moderna es utilizar el modo **DB-less (sin base de datos)**. En lugar de guardar la configuración en PostgreSQL, defines todas las rutas y plugins en un archivo YAML (`kong.yml`) que vive en tu repositorio. Esto se alinea perfectamente con la filosofía de **GitOps** y la arquitectura de monorepo.

A continuación, te muestro cómo estructurar este archivo para implementar Rate Limiting, Validación JWT y Enrutamiento hacia tu NestJS BFF.

## 1. Archivo Declarativo (`kong.yml`)

Este es el esqueleto principal que define los servicios (tu backend de NestJS), las rutas (qué URLs exponen) y los plugins (reglas de infraestructura).

```yaml
_format_version: "3.0"
_transform: true

# 1. DEFINICIÓN DEL SERVICIO (Tu NestJS BFF)
services:
  - name: nestjs-bff-service
    url: http://nestjs-bff:3000 # La URL interna de tu contenedor NestJS
    connect_timeout: 60000
    read_timeout: 60000
    write_timeout: 60000
    
    # 2. DEFINICIÓN DE LAS RUTAS
    routes:
      - name: frontend-api-route
        paths:
          - /api/v1
        strip_path: false # Mantiene el '/api/v1' cuando le pasa la petición a NestJS

    # 3. PLUGINS APLICADOS A ESTE SERVICIO
    plugins:
      # A. Rate Limiting (Prevención de abusos/DDoS)
      - name: rate-limiting
        enabled: true
        config:
          second: 10      # Máximo 10 peticiones por segundo por IP
          minute: 100     # Máximo 100 peticiones por minuto por IP
          policy: local   # Almacena los contadores en la memoria de Kong
          fault_tolerant: true
          hide_client_headers: false

      # B. Validación de JWT (Seguridad)
      - name: jwt
        enabled: true
        config:
          key_claim_name: kid
          claims_to_verify:
            - exp # Verifica que el token no haya expirado
          run_on_preflight: false # No pide token para las peticiones OPTIONS (CORS)

      # C. CORS (Cross-Origin Resource Sharing)
      - name: cors
        enabled: true
        config:
          origins:
            - "https://misistema-ums.com"
          methods:
            - GET
            - POST
            - PUT
            - DELETE
            - OPTIONS
          headers:
            - Accept
            - Accept-Version
            - Content-Length
            - Content-MD5
            - Content-Type
            - Date
            - Authorization
          exposed_headers:
            - X-Auth-Token
          credentials: true
          max_age: 3600

# 4. CONSUMIDORES Y CREADENCIALES (Para validar los JWT)
# Aquí Kong sabe cuáles son los secretos válidos para firmar tokens.
consumers:
  - username: frontend-app
    jwt_secrets:
      - key: "mi-frontend-app-key"
        secret: "super-secreto-compartido-con-el-auth-server" # En producción se inyecta por variable de entorno
        algorithm: HS256
```

## 2. Explicación del Flujo (Cómo se integra)

1. **El Cliente hace una petición:** El frontend en React hace un `POST /api/v1/orders` e incluye un `Authorization: Bearer <token>`.
2. **Kong intercepta (Tier 1):**
    * **Rate Limiting:** Verifica si la IP del cliente no ha excedido las 10 peticiones por segundo. Si las excede, Kong devuelve un `429 Too Many Requests` inmediatamente. *NestJS ni siquiera se entera.*
    * **CORS:** Resuelve los preflights (`OPTIONS`) sin cargar al backend.
    * **JWT:** Abre el token, verifica que la firma coincida con el secreto y que el token no haya expirado (`exp`). Si es inválido, Kong devuelve un `401 Unauthorized`. *NestJS ni siquiera se entera.*
3. **Paso al BFF (Tier 2):** Si todas las verificaciones pasan, Kong reenvía la petición HTTP intacta (con el JWT en el header) hacia `http://nestjs-bff:3000/api/v1/orders`.
4. **NestJS actúa:** NestJS recibe una petición pre-validada. Solo tiene que leer el payload del JWT (para saber quién es el usuario, ya que Kong ya garantizó que el token es legal) y proceder a orquestar las llamadas a los microservicios (TMS, WMS, etc.).

## 3. ¿Cómo pasar la información de Kong a NestJS?

Por defecto, cuando Kong valida un JWT, inyecta headers adicionales antes de mandarle la petición a NestJS. Puedes configurar Kong para que pase el Consumer ID o los claims del JWT en headers específicos:

```yaml
      - name: request-transformer
        enabled: true
        config:
          add:
            headers:
              - x-consumer-id:$(consumer.id)
              - x-credential-username:$(credential.username)
```

En tu código de **NestJS**, en lugar de volver a validar la criptografía del token (lo cual gasta CPU), simplemente confías en Kong y lees los headers en tu AuthGuard:

```typescript
// NestJS: KongAuthGuard.ts
import { Injectable, CanActivate, ExecutionContext } from '@nestjs/common';

@Injectable()
export class KongAuthGuard implements CanActivate {
  canActivate(context: ExecutionContext): boolean {
    const request = context.switchToHttp().getRequest();
    // Kong ya validó que el token existe y no está modificado.
    // Solo leemos el header que Kong inyectó.
    const consumerId = request.headers['x-consumer-id'];
    
    if (!consumerId) {
      return false; // Por si alguien logra saltarse Kong
    }
    
    return true;
  }
}
```

## Resumen de Beneficios
* **Ahorro de CPU en Node.js:** La criptografía (validar JWTs) es pesada en Node.js. Al delegarla a Kong (escrito en C/Lua), tu BFF puede manejar muchos más requests concurrentes.
* **Seguridad como Infraestructura:** Si en el futuro agregas otro servicio (ej. Python o Go), Kong los protegerá con las mismas reglas de JWT y Rate Limiting sin tener que reescribir la lógica de seguridad.
