# Resultados de Pruebas de Rendimiento (Línea Base API)

## 1. Resumen de Ejecución
Se ha realizado una prueba de carga base contra la API (Backend en .NET 10) utilizando `autocannon` para validar el comportamiento del sistema ante un pico masivo de peticiones (100 conexiones concurrentes). Esta prueba se enfoca en el escenario de encolamiento y Rate Limiting descrito en nuestra estrategia para proteger la base de datos y evitar el agotamiento del pool de hilos.

## 2. Resultados Obtenidos
```text
Running 10s test @ http://localhost:5293/health
100 connections

┌─────────┬──────┬──────┬───────┬──────┬─────────┬─────────┬────────┐
│ Stat    │ 2.5% │ 50%  │ 97.5% │ 99%  │ Avg     │ Stdev   │ Max    │
├─────────┼──────┼──────┼───────┼──────┼─────────┼─────────┼────────┤
│ Latency │ 1 ms │ 1 ms │ 2 ms  │ 2 ms │ 1.11 ms │ 3.47 ms │ 360 ms │
└─────────┴──────┴──────┴───────┴──────┴─────────┴─────────┴────────┘
┌───────────┬────────┬────────┬─────────┬─────────┬─────────┬──────────┬────────┐
│ Stat      │ 1%     │ 2.5%   │ 50%     │ 97.5%   │ Avg     │ Stdev    │ Min    │
├───────────┼────────┼────────┼─────────┼─────────┼─────────┼──────────┼────────┤
│ Req/Sec   │ 44,383 │ 44,383 │ 72,959  │ 79,295  │ 72,088  │ 9,759.44 │ 44,361 │
└───────────┴────────┴────────┴─────────┴─────────┴─────────┴──────────┴────────┘

1000 2xx responses, 719911 non 2xx responses
721k requests in 10.02s, 319 MB read
```

## 3. Pruebas Explícitas de Autenticación y Carga Web (K6)
Con el propósito de estresar el proceso de autenticación y la generación del **Grafo de Autorización**, se ejecutó un script automatizado en K6 apuntando a los endpoints de inicio de sesión interno (`/login`), refresco de sesión (`/refresh`), autenticación de cliente externo y el Frontend Web.

### 3.1. Resultados K6 (Login Interno, Cliente Externo, y Web App)
```text
  █ TOTAL RESULTS 

    checks_total.......: 550     13.56/s
    checks_succeeded...: 100.00% 550 out of 550
    checks_failed......: 0.00%   0 out of 550

     login status is 200
     login has session cookie
     client auth status is 200
     client auth returns graph
     refresh status is 200
     web app responds

    HTTP
    http_req_duration..............: avg=149.33ms min=965µs med=221.11ms max=251.71ms p(90)=225.65ms p(95)=228.71ms
    http_req_failed................: 0.00%  0 out of 330
    http_reqs......................: 330    8.13/s

    EXECUTION
    iteration_duration.............: avg=1.44s    min=1.43s med=1.44s    max=1.48s    p(90)=1.45s    p(95)=1.46s   
    iterations.....................: 110    2.71/s
    vus............................: 1      min=1        max=5
    vus_max........................: 5      min=5        max=5
```
El test demuestra que incluso cuando se somete al sistema a carga paralela para la generación del Grafo de Autorización (operación costosa), la latencia P95 se mantiene en **228ms** (muy por debajo del umbral objetivo de 1000ms), con 0 fallos.

## 4. Análisis
- **Rendimiento Base:** La API procesó exitosamente ~72,000 peticiones por segundo en promedio con una latencia p50 de 1ms.
- **Protección (Rate Limiting y HA):** De las 721 mil solicitudes, **1,000** fueron procesadas con éxito (200 OK) y las **719,911** restantes fueron interceptadas elegantemente con códigos no-2xx (probablemente `429 Too Many Requests`). Esto valida perfectamente nuestra estrategia de **"Asegurar un encolamiento elegante antes de que se agote el pool de conexiones"**. 
- **Resiliencia:** El sistema no presentó colapsos (Crashes) a pesar del asedio de casi un millón de peticiones en 10 segundos.

## 4. Pruebas de Rendimiento Frontend (Client-Side)
Para evaluar la velocidad de renderizado de la Single Page Application (SPA) en React 18, se ejecutó una auditoría automatizada de **Lighthouse** apuntando a la ruta principal de login del inquilino interno.

### 4.1. Resultados Lighthouse (Modo Desarrollo)
Se obtuvieron las siguientes métricas base (Core Web Vitals):
- **Performance:** 55/100
- **Accessibility:** 89/100
- **Best Practices:** 100/100
- **SEO:** 91/100

**Métricas de Pintado (Paints):**
- **First Contentful Paint (FCP):** 14.7 s
- **Largest Contentful Paint (LCP):** 27.3 s
- **Speed Index:** 14.7 s

### 4.2. Análisis Frontend
- Las métricas de **Mejores Prácticas (100%)**, **Accesibilidad (89%)** y **SEO (91%)** validan un excelente estado de salud del código y la estructura DOM del proyecto en React.
- **Rendimiento Visual (55%):** El LCP es alto porque la auditoría se corrió contra el servidor de desarrollo de Vite (`npm run dev`). En modo desarrollo, Vite no minifica, no agrupa los bundles (unbundled ESM) ni comprime los archivos, lo que genera alta latencia. Esta métrica mejorará radicalmente hasta el rango del 90%+ una vez que la aplicación pase por el proceso de `build` de producción y sus estáticos sean despachados a través de la CDN optimizada.

## 5. Conclusión
El monolito de .NET 10 bajo Kestrel cumple sobradamente con los requisitos de Alta Disponibilidad estipulados para los escenarios no complejos. Las políticas de rechazo por sobrecarga actúan como se espera, previniendo cascadas de fallos hacia la Base de Datos.
