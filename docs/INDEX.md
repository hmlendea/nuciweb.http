# NuciWeb.HTTP Documentation

**Repository:** [hmlendea/nuciweb.http](https://github.com/hmlendea/nuciweb.http)
**Last Reviewed:** 2026-10-13
**Coverage:** Complete

---

## 📖 Repository Summary

NuciWeb.HTTP is a lightweight .NET 10 library providing utilities for HTTP communication and network operations. It offers:

- **HttpClient creation** with realistic User-Agent headers (dynamically fetched, cached, or custom)
- **Internet connectivity detection** via concurrent ICMP, TCP, and HTTPS probes with fallback endpoints
- **Public IPv4 address retrieval** through multiple privacy-respecting providers with caching
- **Reverse DNS lookups** with in-memory caching

The library is distributed as a NuGet package and targets .NET 10.0.

---

## 🗂️ Root Document Map

| Document | Location | Purpose |
|----------|----------|---------|
| [ARCHITECTURE.md](../ARCHITECTURE.md) | Repository root | High-level architecture, components, data flow, dependencies |
| [SECURITY.md](../SECURITY.md) | Repository root | Security policy, vulnerability reporting, supported versions |
| [PRIVACY.md](../PRIVACY.md) | Repository root | Data handling, third-party providers, user obligations |
| [CONTRIBUTING.md](../CONTRIBUTING.md) | Repository root | Contribution guidelines, code standards, PR process |
| [LICENSE](../LICENSE) | Repository root | GPL-3.0-or-later license terms |
| [README.md](../README.md) | Repository root | Project overview, usage examples, installation |

---

## 📚 Documentation Catalogue

### Architecture & Design

| File | Description |
|------|-------------|
| [architecture.md](./architecture.md) | Detailed architecture complementing root ARCHITECTURE.md |
| [design-decisions.md](./design-decisions.md) | Key architectural choices and rationale |
| [repository-overview.md](./repository-overview.md) | Repository purpose, scope, entry points |
| [repository-structure.md](./repository-structure.md) | Source tree layout, module organisation |

### Components & Capabilities

| File | Description |
|------|-------------|
| [components/http-client-creator.md](./components/http-client-creator.md) | HttpClientCreator static facade deep dive |
| [components/user-agent-fetcher.md](./components/user-agent-fetcher.md) | UserAgentFetcher implementation and caching |
| [components/network-utils.md](./components/network-utils.md) | NetworkUtils static facade deep dive |
| [components/abstractions.md](./components/abstractions.md) | IUserAgentFetcher interface and extensibility |

### Execution Flows

| File | Description |
|------|-------------|
| [flows/http-client-creation.md](./flows/http-client-creation.md) | End-to-end HttpClient creation flow |
| [flows/connectivity-detection.md](./flows/connectivity-detection.md) | Race-to-success connectivity probing |
| [flows/public-ip-retrieval.md](./flows/public-ip-retrieval.md) | Public IP address discovery with fallback |
| [flows/reverse-dns-lookup.md](./flows/reverse-dns-lookup.md) | Hostname resolution and caching |

### Cross-Cutting Concerns

| File | Description |
|------|-------------|
| [configuration.md](./configuration.md) | Hardcoded settings, timeouts, cache durations |
| [concurrency-and-scheduling.md](./concurrency-and-scheduling.md) | Threading, async patterns, race-to-success |
| [error-handling.md](./error-handling.md) | Error taxonomy, handling patterns, recovery |
| [logging.md](./logging.md) | Observability (currently none implemented) |
| [security.md](./security.md) | Security model, threats, mitigations |
| [state-and-persistence.md](./state-and-persistence.md) | In-memory caching, lifecycle, expiry |

### External Integration

| File | Description |
|------|-------------|
| [dependencies.md](./dependencies.md) | External and internal dependencies |
| [integrations.md](./integrations.md) | Third-party service integrations |

### Development & Operations

| File | Description |
|------|-------------|
| [build-and-deployment.md](./build-and-deployment.md) | Build pipeline, NuGet packaging, CI/CD |
| [testing.md](./testing.md) | Test strategy, organisation, coverage |
| [change-guide.md](./change-guide.md) | How to modify common areas safely |
| [troubleshooting.md](./troubleshooting.md) | Common issues and solutions |
| [documentation-maintenance.md](./documentation-maintenance.md) | How to keep docs current |

### Reference

| File | Description |
|------|-------------|
| [api-reference/INDEX.md](./api-reference/INDEX.md) | API reference index with endpoint summary |
| [api-reference/httpclientcreator.md](./api-reference/httpclientcreator.md) | HttpClientCreator API documentation |
| [api-reference/networkutils.md](./api-reference/networkutils.md) | NetworkUtils API documentation |
| [api-reference/iuseragentfetcher.md](./api-reference/iuseragentfetcher.md) | IUserAgentFetcher interface documentation |
| [api-usage-examples.md](./api-usage-examples.md) | Example requests/responses for API usage |
| [data-model.md](./data-model.md) | Domain entities, cache structures |
| [invariants.md](./invariants.md) | System-wide invariants and contracts |
| [ambiguities-and-open-questions.md](./ambiguities-and-open-questions.md) | Unresolved items, TODOs, known gaps |
| [faq.md](./faq.md) | Frequently asked questions |

---

## 🧭 Navigation Aids

| Audience | Start Here |
|----------|------------|
| **Newcomers** | [quick-start.md](./quick-start.md) → [repository-overview.md](./repository-overview.md) → [architecture.md](./architecture.md) |
| **Component Owners** | [components/](./components/) → [design-decisions.md](./design-decisions.md) → [change-guide.md](./change-guide.md) |
| **Debuggers** | [flows/](./flows/) → [error-handling.md](./error-handling.md) → [troubleshooting.md](./troubleshooting.md) |
| **Integrators** | [api-reference/INDEX.md](./api-reference/INDEX.md) → [api-usage-examples.md](./api-usage-examples.md) → [integrations.md](./integrations.md) |

---

## 🔧 Maintenance Metadata

- **Generated:** 2026-10-13
- **Source Commit:** Current working tree
- **Validation:** All tests pass (84/84)
- **Build:** `dotnet build NuciWeb.HTTP.slnx` succeeds
- **Test:** `dotnet test NuciWeb.HTTP.slnx` succeeds