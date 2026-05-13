# ADR 0013: Cloud Infrastructure Topology and Disaster Recovery (DR) Strategy

## Status
Proposed

## Date
2026-05-08

## Context
Mission-critical logistics operations (e.g., vessel unloading, gate dispatches) require 24/7 availability (99.9% SLA). A failure in the primary datacenter cannot cause prolonged operational downtime. The current architecture uses Docker containers, but the overarching cloud topology and failover mechanisms are undefined.

## Decision
We will design the infrastructure to be Cloud-Native and highly available using active/passive (or active/active) multi-region strategies:

1. **Container Orchestration**: The UMS workloads will be deployed on a managed orchestrator (e.g., Azure Kubernetes Service or Azure Container Apps) capable of Horizontal Pod Autoscaling (HPA) to dynamically react to traffic spikes.
2. **Multi-AZ and Region Failover**: Deployments will span multiple Availability Zones (Multi-AZ). A secondary standby region will be configured for Disaster Recovery (DR).
3. **Global Load Balancing**: Implement a global entry point (e.g., Azure Front Door or Cloudflare) to route traffic intelligently and instantly failover to the DR region if the primary region goes offline.

## Consequences
* **Pros**: Guarantees business continuity and high availability during critical regional outages.
* **Cons**: Doubles infrastructure costs (for active/active) and increases CI/CD pipeline and IaC (Infrastructure as Code) complexity.
