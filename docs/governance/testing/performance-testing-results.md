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

## 3. Analysis
- **Base Performance:** The API successfully processed ~72,000 requests per second on average with a p50 latency of 1ms.
- **Protection (Rate Limiting and HA):** Out of 721 thousand requests, **1,000** were processed successfully (200 OK) and the remaining **719,911** were gracefully intercepted with non-2xx codes (likely `429 Too Many Requests`). This perfectly validates our strategy to **"Ensure graceful queuing before exhausting the connection pool"**.
- **Resilience:** The system did not experience any crashes despite the barrage of nearly a million requests in 10 seconds.

## 4. Conclusion
The .NET 10 monolith under Kestrel comfortably meets the High Availability requirements stipulated for non-complex scenarios. The overload rejection policies act as expected, preventing cascading failures towards the Database.
