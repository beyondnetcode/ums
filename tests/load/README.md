# UMS Load Tests — k6

Performance baseline scripts for critical API endpoints.

## Prerequisites

```bash
brew install k6          # macOS
# or: https://k6.io/docs/getting-started/installation/
```

## Running

```bash
# Smoke test (1 VU, 30s) — verify the script runs without errors
k6 run --env BASE_URL=http://localhost:5000 smoke.js

# Load test — production-like traffic
k6 run --env BASE_URL=http://localhost:5000 --env TOKEN=<jwt> tenants.js

# Stress test — find breaking point
k6 run --env BASE_URL=http://localhost:5000 --env TOKEN=<jwt> stress.js

# With a summary output file
k6 run --out json=results.json tenants.js
```

## Thresholds (defined per script)

| Metric | Target |
|---|---|
| http_req_duration p(95) | < 500 ms |
| http_req_duration p(99) | < 1 500 ms |
| http_req_failed | < 1% |
| http_reqs (RPS) | > 100/s under load |
