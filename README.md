Sprint 1 – Summary

This sprint delivers the backend foundation for an insurance building management system, focused on clients, buildings, and geographic data.

The solution is built using ASP.NET Core, Entity Framework Core, and MediatR, following a clean, layered architecture with clear separation between API, application logic, domain models, and persistence.

Broker-facing REST APIs were implemented to:

- manage clients (create, update, search, retrieve details)

- register and manage buildings owned by clients

- retrieve geographic reference data (countries, counties, cities)

Business logic is handled in the application layer via MediatR handlers, while controllers remain thin.
Validation and error handling are centralized using FluentValidation and a custom Exception Middleware, removing error-handling concerns from controllers.

Data is persisted using EF Core with SQLite, including proper entity relationships and foreign key constraints.
AutoMapper is used to map between domain entities and API DTOs, including custom mappings for derived data.

Basic unit tests were added to verify core business logic in isolation, using an in-memory database.
The architecture is prepared for future sprints, including authentication, authorization, policies, pricing logic, and reporting.