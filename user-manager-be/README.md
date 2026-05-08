<div align="center">
  <img src="https://raw.githubusercontent.com/dotnet/brand/main/logo/dotnet-logo.png" alt="Logo" width="120" />

  <h1>🚀 Celo User Manager Service</h1>
  <p>
    <b>Centralized Identity, Authentication & Authorization System</b>
  </p>
  <p>
    <i>Built with <b>Domain-Driven Design (DDD)</b>, <b>Clean Architecture</b> & <b>CQRS</b></i>
  </p>

  <!-- Badges -->
  <p>
    <img src="https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white" alt=".NET 10" />
    <img src="https://img.shields.io/badge/Architecture-Clean%20%7C%20DDD-success" alt="DDD Architecture" />
    <img src="https://img.shields.io/badge/Pattern-CQRS%20%7C%20MediatR-blue" alt="CQRS" />
    <img src="https://img.shields.io/badge/Database-PostgreSQL-336791?logo=postgresql&logoColor=white" alt="PostgreSQL" />
    <img src="https://img.shields.io/badge/Auth-OpenIddict%20%7C%20OAuth2-orange" alt="OAuth2" />
  </p>
</div>

---

## 🌟 Overview

The **User Manager Service** is a core backend service acting as a centralized **Identity Provider (IdP)** and multi-tenant application manager. It is designed to serve large software ecosystems where multiple applications share a unified user database and a Single Sign-On (SSO) authentication mechanism.

This source code is an exemplary implementation of **Domain-Driven Design (DDD)** in the real world, featuring strict boundaries, no persistence logic leakage, and complete isolation of framework specifics (such as EF Core and system time) from the core business logic.

---

## ✨ Core Features

### 🔐 Authentication & Authorization
* **Single Sign-On (SSO)**: Centralized authentication via OAuth2 & OpenID Connect (powered by OpenIddict).
* **Multi-tenant Applications**: Manage multiple client applications. Users can grant third-party applications access to their profiles via Consent.
* **Magic Link Login**: Secure, passwordless login via email links.
* **Refresh Token Rotation**: Enhanced security mechanism against token theft.

### 👥 User Management & Roles
* Comprehensive account state management (Active, Banned) and Email Verification.
* Flexible authorization structure: **Roles** (Admin, User) combined with fine-grained **Permissions**.
* **SSO Accounts**: Special accounts designed exclusively for specific integrated applications.

### 💳 Subscription & Licensing
* Manage Subscription Plans, allowing users to upgrade their account tiers.
* Track License Packages and enforce system resource limits based on subscription tiers.

---

## 🏛️ Architecture Highlights

The project strictly adheres to **Clean Architecture** to ensure long-term maintainability and technological flexibility:

```text
user-manager-be/
├── UserService.Domain        👉 [Heart] Entities, Aggregates, Value Objects, Exceptions
├── UserService.Application   👉 [Brain] CQRS (Commands/Queries), Handlers, DTOs
├── UserService.Infrastructure👉 [Limbs] DbContext, Interceptors, Repositories, SystemClock
└── UserService.Api           👉 [Face] Controllers, Exception Middleware, Policies, OpenAPI
```

### Technical Highlights:
1. **Rich Domain Model & Aggregate Roots**: Entities encapsulate their state (hidden setters, Factory Methods). Repositories are strictly limited to Aggregate Roots.
2. **CQRS with MediatR**: Complete separation of Read (Queries) and Write (Commands) operations. The Application layer orchestrates everything via Handlers.
3. **Strict DDD Rules**: 
   - No LINQ `Expression` leaking into `IRepository` to protect the Application layer.
   - Audit logging (`CreatedAt`, `UpdatedAt`) is managed automatically via EF Core Interceptors.
4. **Deterministic Time**: Hidden `DateTime.UtcNow` calls are eliminated. All time-related operations are injected via the `IClock` interface.
5. **Unified Exception Handling**: Custom Domain Exceptions are thrown and centrally mapped to appropriate HTTP Status Codes via `ExceptionHandlingMiddleware`.

---

## 🛠️ Tech Stack

* **Framework:** .NET 10 (C# 14)
* **Web API:** ASP.NET Core, Swagger/OpenAPI
* **Architecture:** Clean Architecture, CQRS, Domain-Driven Design (DDD)
* **Database:** PostgreSQL (with Entity Framework Core)
* **Identity & Security:** ASP.NET Core Identity, OpenIddict (OAuth 2.0 / OIDC), JWT
* **Key Libraries:** MediatR (CQRS), FluentValidation, Mapster/AutoMapper.

---

## 🚀 Getting Started

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* PostgreSQL Server
* Visual Studio 2022 / JetBrains Rider / VS Code

### Installation
1. **Clone the repository:**
   ```bash
   git clone <repository_url>
   cd user-manager-be
   ```

2. **Database Configuration:**
   Open `appsettings.Development.json` (or use User Secrets) and update the PostgreSQL connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=user_manager_db;Username=postgres;Password=yourpassword"
   }
   ```

3. **Run Migrations:**
   ```bash
   dotnet ef database update --project UserService.Infrastructure --startup-project UserService.Api
   ```

4. **Run the Application:**
   ```bash
   cd UserService.Api
   dotnet run
   ```
   The application will start at: `https://localhost:5001`. API documentation is available at `/swagger`.

---

## 🤝 Contributing

As the project follows DDD and Clean Architecture, please note:
1. All core business logic must reside in `UserService.Domain`.
2. Do not introduce Data Access packages (like EF Core) into the `Application` or `Domain` layers.
3. When creating a new Entity, inherit from `AggregateRoot<T>` if it stands alone, or `Entity<T>` if it belongs to another Aggregate.

---

<p align="center">
  <i>Built with a passion for clean software architecture and long-term maintainability.</i>
</p>
