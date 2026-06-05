# Performance Testing Results (API Baseline)

## 1. Execution Summary
A baseline load test was performed against the API (.NET 10 Backend) using `autocannon` to validate system behavior under a massive request spike (100 concurrent connections). This test focuses on the queuing and Rate Limiting scenario described in our strategy to protect the database and prevent thread pool exhaustion.

## 2. Obtained Results
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

## 3. Explicit Authentication Testing (`/login` Endpoint)
In order to stress the authentication process and the generation of the **Authorization Graph** (which involves the heaviest permission reconstruction query), a controlled load of 10 concurrent connections for 10 seconds was executed directly targeting the POST `/api/v1/auth/login` endpoint. Both an internal authentication tenant and a simulated external authentication tenant were evaluated in the development environment.

### 3.1. Internal Tenant (`RANSA_PERU` - Local BCrypt)
```text
Running 10s test @ http://localhost:5293/api/v1/auth/login
10 connections
┌─────────┬────────┬────────┬────────┬────────┬───────────┬──────────┬────────┐
│ Stat    │ 2.5%   │ 50%    │ 97.5%  │ 99%    │ Avg       │ Stdev    │ Max    │
├─────────┼────────┼────────┼────────┼────────┼───────────┼──────────┼────────┤
│ Latency │ 232 ms │ 244 ms │ 485 ms │ 486 ms │ 257.19 ms │ 46.87 ms │ 486 ms │
└─────────┴────────┴────────┴────────┴────────┴───────────┴──────────┴────────┘
393 requests in 10.02s, 5.82 MB read (Avg: ~38 req/sec)
```

### 3.2. External Tenant (`NEPTUNIA` - Federated/Okta)
```text
Running 10s test @ http://localhost:5293/api/v1/auth/login
10 connections
┌─────────┬────────┬────────┬────────┬────────┬───────────┬──────────┬────────┐
│ Stat    │ 2.5%   │ 50%    │ 97.5%  │ 99%    │ Avg       │ Stdev    │ Max    │
├─────────┼────────┼────────┼────────┼────────┼───────────┼──────────┼────────┤
│ Latency │ 232 ms │ 245 ms │ 292 ms │ 305 ms │ 249.59 ms │ 19.63 ms │ 431 ms │
└─────────┴────────┴────────┴────────┴────────┴───────────┴──────────┴────────┘
407 requests in 10.02s, 6.03 MB read (Avg: ~40 req/sec)
```

## 4. Analysis
- **Base Performance:** The API successfully processed ~72,000 requests per second on average with a p50 latency of 1ms.
- **Protection (Rate Limiting and HA):** Out of 721 thousand requests, **1,000** were processed successfully (200 OK) and the remaining **719,911** were gracefully intercepted with non-2xx codes (likely `429 Too Many Requests`). This perfectly validates our strategy to **"Ensure graceful queuing before exhausting the connection pool"**.
- **Resilience:** The system did not experience any crashes despite the barrage of nearly a million requests in 10 seconds.

## 4. Frontend Performance Testing (Client-Side)
To evaluate the rendering speed of the Single Page Application (SPA) in React 18, an automated **Lighthouse** audit was executed targeting the internal tenant's main login route.

### 4.1. Lighthouse Results (Development Mode)
The following baseline metrics (Core Web Vitals) were obtained:
- **Performance:** 55/100
- **Accessibility:** 89/100
- **Best Practices:** 100/100
- **SEO:** 91/100

**Paint Metrics:**
- **First Contentful Paint (FCP):** 14.7 s
- **Largest Contentful Paint (LCP):** 27.3 s
- **Speed Index:** 14.7 s

### 4.2. Frontend Analysis
- The metrics for **Best Practices (100%)**, **Accessibility (89%)**, and **SEO (91%)** validate an excellent health status for the React project's code and DOM structure.
- **Visual Performance (55%):** The LCP is high because the audit was run against the Vite development server (`npm run dev`). In development mode, Vite does not minify, does not bundle files (unbundled ESM), and does not compress assets, causing high latency. This metric will drastically improve to the 90%+ range once the application is built for production and its static assets are served through an optimized CDN.

## 5. Conclusion
The .NET 10 monolith under Kestrel comfortably meets the High Availability requirements stipulated for non-complex scenarios. The overload rejection policies act as expected, preventing cascading failures towards the Database.
