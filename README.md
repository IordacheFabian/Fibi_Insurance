🏢 **Insurance Backend API**

A modern backend for managing end-to-end building insurance workflows, built with Clean Architecture, CQRS, and production-ready patterns.

## 🚀 Overview

This service powers the complete policy lifecycle for an insurance organization.

**Primary roles**
- 👨‍💼 **Brokers:** handle clients, buildings, policies, payments, and claims.
- 🛠️ **Administrators:** curate metadata, manage brokers, drive reporting, and adjudicate claims.

## ⚙️ Capabilities

- 👤 Manage clients (individuals or companies).
- 🏢 Register insured buildings and link them to policies.
- 📄 Create, activate, and version insurance policies.
- 💰 Calculate premiums via fees and risk factors.
- 🔄 Track endorsements with full version history.
- 💳 Record premium payments and outstanding balances.
- 📢 Submit, review, and resolve claims.
- 📊 Produce aggregated reports by geography, broker, or portfolio segment.
- 🔐 Enforce JWT auth with role-based authorization.

## 🧩 Core Entities

Client · Building · Policy · PolicyVersion · PolicyEndorsement · Payment · Claim · Broker · Currency · FeeConfiguration · RiskFactorConfiguration · Country / County / City · User / Role

## 🔄 Business Flow

1. Create a client and register one or more buildings.
2. Issue a policy; the system computes the premium.
3. Activate the policy and begin coverage.
4. Apply endorsements to spawn new versions while preserving history.
5. Collect payments and reconcile balances.
6. Submit claims and process resolutions.
7. Generate operational and strategic reports.

```
Client → Building → Policy → Activation
                   ↓
             Endorsements
                   ↓
           Payments & Claims
                   ↓
               Reporting
```

## 🏗️ Architecture

```
API (ASP.NET Core)
│
├── Application        → CQRS, MediatR handlers, DTOs
├── Domain             → Core entities and invariants
├── Persistence        → EF Core (SQLite), DbContext, repositories
├── Infrastructure     → JWT, security services, external adapters
│
├── Application.Tests
└── API.IntegrationTests
```

## 🧠 Key Concepts

- CQRS with MediatR pipelines.
- AutoMapper-driven mapping profiles.
- EF Core (SQLite) persistence layer.
- Centralized exception middleware.
- JWT authentication with RBAC.
- Versioned aggregates (`PolicyVersion`).

## 🔐 Security & Access

- 🔑 JWT authentication for every request.
- 🛡️ Role-based policies for Broker vs Admin scopes.
- 🔀 Route segmentation: `/api/brokers/*` vs `/api/admin/*`.

## 📡 API Highlights

**Broker endpoints**
- GET `/api/brokers/policies`
- POST `/api/brokers/policies`
- POST `/api/brokers/policies/{id}/endorsements`
- POST `/api/brokers/policies/{id}/payments`
- POST `/api/brokers/policies/{id}/claims`

**Admin endpoints**
- GET `/api/admin/reports/*`
- POST `/api/admin/brokers`
- PUT `/api/admin/currencies`
- POST `/api/admin/claims/{id}/approve`

## 🧪 Testing Strategy

- ✅ Unit tests for calculations, transitions, and validation rules.
- ✅ Integration tests for critical API flows and scenario coverage.

## 🛠️ Tech Stack

ASP.NET Core · Entity Framework Core (SQLite) · MediatR · AutoMapper · FluentValidation · xUnit · Swashbuckle

## ▶️ Getting Started

```bash
dotnet restore
dotnet build
dotnet run

# Tests
dotnet test

# Apply migrations
dotnet ef database update
```

## 📌 Notes

- Designed for extensibility, maintainability, and clear separation between layers.
- Business logic lives in the `Application` layer; controllers remain thin.
- Supports real-world insurance needs: versioning, payments, claims, reporting.

## ⭐ Future Improvements

- Swagger examples or Postman collection.
- Docker-based local environment.
- CI/CD automation.
- Caching and performance tuning.