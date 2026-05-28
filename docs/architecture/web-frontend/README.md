# UMS React Web Frontend Applied Reference

> Language: [English](./README.md) | [Espanol](./README.es.md)

This section documents how the UMS React Web implementation applies the Evolith Web Frontend Standard - React.

UMS is an applied reference, not the source of universal frontend standards. Reusable rules belong in Evolith. UMS keeps concrete product routes, local API headers, product modules, source evidence, and implementation-specific adaptations.

## Documents

| Document | Purpose |
|---|---|
| [UMS React Applied Reference](./ums-react-applied-reference.md) | Evidence-based mapping between Evolith React topics and UMS source files. |

## Authority boundary

| Concern | Evolith | UMS |
|---|---|---|
| Reusable standards | Owns principles, boilerplate rules, quality gates, and promotion criteria | Consumes standards |
| Product implementation | References examples only | Owns concrete source code and local decisions |
| UI design system | Owns token governance and accessibility rules | Owns actual UMS token values and components |
| Data access | Owns reusable boundary requirements | Owns UMS API URLs, headers, CSRF behavior, and tenant strategy |

## Current evidence scope

This reference is based on the current React app under:

```text
src/apps/ums.web-app
```

Key observed sources include app bootstrap, routing, layout shell, HTTP context/client, Tailwind configuration, and Material Design 3 token definitions.

---
[Back to Architecture Portal](../index.md)
