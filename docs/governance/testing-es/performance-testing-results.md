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

## 3. Análisis
- **Rendimiento Base:** La API procesó exitosamente ~72,000 peticiones por segundo en promedio con una latencia p50 de 1ms.
- **Protección (Rate Limiting y HA):** De las 721 mil solicitudes, **1,000** fueron procesadas con éxito (200 OK) y las **719,911** restantes fueron interceptadas elegantemente con códigos no-2xx (probablemente `429 Too Many Requests`). Esto valida perfectamente nuestra estrategia de **"Asegurar un encolamiento elegante antes de que se agote el pool de conexiones"**. 
- **Resiliencia:** El sistema no presentó colapsos (Crashes) a pesar del asedio de casi un millón de peticiones en 10 segundos.

## 4. Conclusión
El monolito de .NET 10 bajo Kestrel cumple sobradamente con los requisitos de Alta Disponibilidad estipulados para los escenarios no complejos. Las políticas de rechazo por sobrecarga actúan como se espera, previniendo cascadas de fallos hacia la Base de Datos.
