# FinTrack API — GitHub Copilot Instructions

## Technology Stack Declaration
- Backend API: ASP.NET Core 8 Web API (C#)
- ORM: Entity Framework Core 8
- Database: Microsoft SQL Server 2022
- Authentication: ASP.NET Core Identity + JWT Bearer Tokens
- Logging: Serilog
- Testing: xUnit + Moq

## Architecture Conventions
Use layered architecture with clear separation of concerns:
1. Controller/Route Layer: handles HTTP request/response, model binding, auth attributes.
2. Service Layer: contains business logic and orchestration only.
3. Repository Layer: contains all data access through EF Core.
4. Model/Entity Layer: contains domain entities and DTO contracts.

Rules:
- Controllers must not contain business or data-access logic.
- Services must not execute raw SQL directly.
- Repositories are the only layer that talks to DbContext.
- Keep dependencies one-way: Controller -> Service -> Repository -> Model/Entity.

## Coding Standards
- Use C# async/await for all I/O operations.
- Use `decimal` for all currency/amount fields (never float/double).
- Use explicit type annotations for public members.
- Use meaningful names for classes, methods, and variables.
- Add XML documentation comments for all public classes and methods.
- Use structured logging with Serilog (`ILogger`) and contextual properties.
- Avoid hard-coded configuration values; use appsettings and options pattern.
- Keep methods focused and small; prefer single responsibility.

## Security Rules
- Protect user-scoped endpoints with authentication and authorization.
- Use JWT claims to identify the current user.
- Enforce ownership checks: users can only access their own transactions/balances.
- Validate all external input (amounts, IDs, participant lists, split type).
- Return safe error responses; do not leak stack traces or sensitive internals.
- Never log secrets, tokens, or sensitive personal financial details.

## Testing Expectations
- Use xUnit for unit tests and Moq for dependency mocking.
- Cover happy paths, validation failures, authorization failures, and edge cases.
- Write deterministic, isolated tests with clear Arrange-Act-Assert structure.
- Add/maintain tests for every business-critical rule before merging.
- Minimum quality bar for new feature work:
  - equal/custom split logic tests
  - invalid custom total validation test
  - net balance calculation test
  - unauthorized access test