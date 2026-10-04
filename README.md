# SolNeat.PolicyEngine

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4.svg)](https://dotnet.microsoft.com/)

A lightweight, high-performance, headless authorization engine for .NET, built on **Domain-Driven Design (DDD)** and **Onion Architecture** principles.

It implements a unified **Actor — Role — Permission — Resource:Action** domain model and follows the enterprise **PAP / PDP / PEP** (Policy Administration, Decision, and Enforcement Points) specification.

## Key Capabilities
* **RFC 9068 & OIDC Ready**: Effortlessly serializes evaluated PDP permissions into JWT access tokens via custom claims (`x-access-permission`) for downstream Resource Server enforcement.
* **OpenIddict & EF Core Integration**: Out-of-the-box PEP pipeline integrations for OpenIddict token issuance and EF Core persistence.
* **Unified Principal Model**: Uniformly handles authorization policies for human users, background services, and programmatic agents (M2M).
---

## Packages & Ecosystem

The suite consists of three modular building blocks:

| Package | Description | NuGet |
| :--- | :--- | :--- |
| **`SolNeat.PolicyEngine`** | Core authorization engine (DDD/Onion, PAP/PDP logic). | [![NuGet](https://img.shields.io/nuget/v/SolNeat.PolicyEngine.svg)](https://www.nuget.org/packages/SolNeat.PolicyEngine) |
| **`SolNeat.PolicyEngine.EF`** | EF Core persistence store & entity configurations. | [![NuGet](https://img.shields.io/nuget/v/SolNeat.PolicyEngine.EF.svg)](https://www.nuget.org/packages/SolNeat.PolicyEngine.EF) |
| **`SolNeat.PolicyEngine.OpenIddict`** | OpenIddict PEP integration (RFC 9068 JWT claims & `x-access-permission` injection). | [![NuGet](https://img.shields.io/nuget/v/SolNeat.PolicyEngine.OpenIddict.svg)](https://www.nuget.org/packages/SolNeat.PolicyEngine.OpenIddict) |

---
## Quick Start

See the [SolNeat.AuthDemo](https://github.com/SolNeat/PolicyEngine.AuthDemo) for a full runnable example.

## License

This project is licensed under the MIT License — see the LICENSE file for details.

## 💬 Feedback & Suggestions

* **Website:** [solneat.com](https://solneat.com)
* **Email:** [hello@solneat.com](mailto:hello@solneat.com)
* **Telegram:** [@solneat_tech](https://t.me/solneat_tech)