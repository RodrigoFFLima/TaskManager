# Task Manager — Full Stack .NET + Angular

A production-quality Task Management system built with Clean Architecture, DDD, and Npgsql raw SQL.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend API | .NET 9, ASP.NET Core Web API |
| Auth Service | .NET 9, ASP.NET Core Web API |
| Database | PostgreSQL 16 (Npgsql raw — no EF/Dapper) |
| Auth | JWT Bearer (BCrypt password hashing) |
| Validation | FluentValidation |
| Testing | xUnit, Moq, FluentAssertions |
| Frontend | Angular 17 standalone |
| Container | Docker + Docker Compose |

## Architecture

```
TaskManager.sln
├── src/
│   ├── TaskManager.Domain          # Entities, Value Objects, Interfaces, Exceptions
│   ├── TaskManager.Application     # Services, DTOs, Validators (no framework deps)
│   ├── TaskManager.Infrastructure  # Npgsql repositories, JwtService, BCrypt
│   ├── TaskManager.Api             # CRUD tasks — port 8080
│   └── TaskManager.Auth            # Register/Login — port 8081
├── tests/
│   ├── TaskManager.Domain.Tests
│   ├── TaskManager.Application.Tests
│   ├── TaskManager.Infrastructure.Tests
│   └── TaskManager.Api.Tests
├── frontend/task-manager-ui        # Angular 17 SPA — port 4200
└── docker-compose.yml
```

**Dependency flow**: Api/Auth → Application → Domain ← Infrastructure

## Quick Start — Docker Compose

```bash
docker-compose up --build
```

Services start at:
- Frontend: http://localhost:4200
- Tasks API + Swagger: http://localhost:8080/swagger
- Auth API + Swagger: http://localhost:8081/swagger

## Quick Start — Local Development

### Prerequisites
- .NET 9 SDK
- PostgreSQL 16 running locally
- Node 20 + npm

### 1. Database
```bash
# Ensure PostgreSQL is running with:
# Database: taskmanager | User: postgres | Password: postgres | Port: 5432
# Tables and seed data are created automatically on first startup
```

### 2. Auth Service (port 8081)
```bash
cd src/TaskManager.Auth
dotnet run
```

### 3. API Service (port 8080)
```bash
cd src/TaskManager.Api
dotnet run
```

### 4. Frontend (port 4200)
```bash
cd frontend/task-manager-ui
npm install
npm start
```

## Demo Credentials

| Field | Value |
|-------|-------|
| Email | `demo@taskmanager.com` |
| Password | `Demo@1234` |

> The demo user and 4 sample tasks are created automatically via seed data on first run.

## API Reference

### Auth API (port 8081)

| Method | Endpoint | Body | Auth |
|--------|----------|------|------|
| POST | `/api/auth/register` | `{ name, email, password }` | — |
| POST | `/api/auth/login` | `{ email, password }` | — |

### Tasks API (port 8080)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/tasks` | List all tasks for current user | Bearer |
| GET | `/api/tasks/{id}` | Get task by ID | Bearer |
| POST | `/api/tasks` | Create task | Bearer |
| PUT | `/api/tasks/{id}` | Update task | Bearer |
| PATCH | `/api/tasks/{id}/status` | Change task status | Bearer |
| DELETE | `/api/tasks/{id}` | Delete task | Bearer |

### Status Transitions

```
Pending → InProgress → Completed
Pending → Cancelled
InProgress → Cancelled
```

### Priority Values
`Low` | `Medium` | `High` | `Critical`

## Running Tests

```bash
# All tests
dotnet test

# With verbose output
dotnet test --logger "console;verbosity=normal"
```

**Results**: 136 tests — 0 failures

| Project | Tests |
|---------|-------|
| Domain.Tests | 50 |
| Application.Tests | 48 |
| Infrastructure.Tests | 15 |
| Api.Tests | 23 (WebApplicationFactory) |

## Key Design Decisions

### Why Npgsql Raw?
Requirement explicitly prohibits EF Core, Dapper, and MediatR. All SQL is written as parameterized `NpgsqlCommand` queries — providing full control and zero ORM magic.

### Why IPasswordHasher Abstraction?
BCrypt lives in Infrastructure. Exposing it directly to Application would violate Clean Architecture (Application cannot depend on Infrastructure). The `IPasswordHasher` interface keeps the dependency boundary clean.

### Domain Status Transitions
`TaskStatusValue` encodes valid workflow transitions as a value object method `CanTransitionTo()`. Invalid transitions throw a `DomainException` before reaching the database.

### Two Separate Services
`TaskManager.Api` (tasks CRUD) and `TaskManager.Auth` (register/login) are separate processes to demonstrate service separation, even though they share the same Application and Infrastructure assemblies.

---

## AI-Assisted Development

This project was built with **Claude Code** (`claude-sonnet-4-6`) — Anthropic's agentic CLI that reads/writes files and runs commands directly in the terminal. Below are the actual prompts used, representative outputs, and an honest evaluation of the results.

---

### Prompts Used

#### Prompt 1 — Full project scaffolding (condensed)

> "Create a Full Stack project for a Full Stack .NET Developer technical test.
> Backend: .NET 9, Clean Architecture, 5 source projects + 4 test projects (xUnit + Moq + FluentAssertions).
> Projects: TaskManager.Domain, TaskManager.Application, TaskManager.Infrastructure, TaskManager.Api (port 8080), TaskManager.Auth (port 8081).
> Rules: Npgsql RAW SQL only — no EF Core, Dapper, or MediatR. JWT Bearer auth, BCrypt hashing, FluentValidation.
> Domain: entities with factory methods, Value Objects (Email, TaskStatusValue with status transition rules).
> Frontend: Angular 17 standalone — Login, Register, task list with full CRUD, JWT interceptor, AuthGuard, Reactive Forms.
> Docker Compose: everything starts with `docker compose up`. Seed data with demo@taskmanager.com / Demo@1234.
> Required docs: USER_STORY.md, README.md, GENAI.md."

> **Note:** The original job spec referenced .NET 8. Since only the .NET 9 SDK was available on the development machine, the project targets `net9.0`. The APIs are identical — no .NET 9-specific features were used.

**What was generated:** The complete project structure — 5 source assemblies, 4 test projects, full Angular SPA, Docker Compose with multi-stage Dockerfiles, all documentation files, 37 tests passing on first `dotnet test` run.

---

#### Prompt 2 — Test coverage expansion

> "Continue with the tests"

**What was generated:** Full test suites for all four test projects:
- `TaskStatusValueTests` — complete 8-case transition matrix as `[Theory]`
- `EmailTests` — normalization, validation, equality
- `TaskItemTests` — factory validation, `ChangeStatus()` domain rules
- `AuthServiceTests` / `TaskServiceTests` — Moq-based application layer tests
- `TasksControllerTests` — WebApplicationFactory integration tests with JWT token helper

---

#### Prompt 3 — Docker Compose debug

> "Docker is running, bring up the docker-compose stack"

**What was generated:** Identified two root causes — macOS AirPlay Receiver occupying port 5000 and an incorrect BCrypt hash in seed data. Remapped services to ports 8080/8081, generated the correct hash via a temp .NET program, updated `DatabaseMigrator.cs`, and patched the live container via `docker exec`.

---

### Sample Output — `TaskStatusValue.cs`

The AI generated this value object encoding all valid workflow transitions as a switch expression:

```csharp
public bool CanTransitionTo(TaskStatusValue next) => (Value, next.Value) switch
{
    (DomainTaskStatus.Pending,    DomainTaskStatus.InProgress) => true,
    (DomainTaskStatus.Pending,    DomainTaskStatus.Cancelled)  => true,
    (DomainTaskStatus.InProgress, DomainTaskStatus.Completed)  => true,
    (DomainTaskStatus.InProgress, DomainTaskStatus.Cancelled)  => true,
    _ => false
};
```

**Evaluation:** Idiomatic C# 9 switch expression, correct business rules, throws `DomainException` before any DB call — no changes needed.

---

### Sample Output — `TestWebApplicationFactory.cs`

The AI designed and generated this test helper to allow full API integration tests without a real PostgreSQL instance:

```csharp
protected override void ConfigureWebHost(IWebHostBuilder builder)
{
    builder.UseEnvironment("Testing");
    builder.ConfigureServices(services =>
    {
        services.RemoveAll<IDatabaseMigrator>();
        services.AddSingleton<IDatabaseMigrator>(new Mock<IDatabaseMigrator>().Object);
        services.RemoveAll<ITaskService>();
        services.AddScoped(_ => TaskServiceMock.Object);
    });
}
```

**Evaluation:** Correctly introduced the `IDatabaseMigrator` interface (which didn't exist yet) to make startup mockable — a non-trivial design decision that required modifying both `DependencyInjection.cs` and `Program.cs` as prerequisites.

---

### Sample Output — Angular Auth Interceptor

```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AuthService).getToken();
  if (!token) return next(req);
  return next(req.clone({
    setHeaders: { Authorization: `Bearer ${token}` }
  }));
};
```

**Evaluation:** Correctly uses Angular 17's functional interceptor API (not the deprecated class-based approach). No changes needed.

---

### Issues Found and Corrected

| Issue | Root Cause | Fix Applied |
|-------|-----------|-------------|
| `BCrypt` called directly in `AuthService` | Application layer importing Infrastructure package — Clean Architecture violation | AI introduced `IPasswordHasher` interface in Application; moved implementation to Infrastructure |
| `TaskStatus` namespace collision | `TaskManager.Domain.Enums.TaskStatus` conflicting with `System.Threading.Tasks.TaskStatus` | AI added `using DomainTaskStatus = ...` alias across all affected files |
| Login didn't redirect after success | `AuthService.storeSession()` saved the token but never called `router.navigate` | Added `this.router.navigate(['/tasks'])` to `storeSession()` |
| Wrong BCrypt hash in seed data | Placeholder hash didn't match `Demo@1234` | Generated correct hash, patched live DB, rebuilt image |
| `WebApplicationFactory` failing without DB | `Program.cs` ran migrations at startup with no mock path | Extracted `IDatabaseMigrator` interface; replaced with mock in test host |

---

### Overall Evaluation

**Strengths of AI-generated code:**
- Architectural discipline was consistent — dependency flow was enforced across all 5 projects without drift
- DDD patterns (factory methods, value objects, domain exceptions) were applied correctly and uniformly
- Test quality was high — the AI chose `WebApplicationFactory` + Moq over simple unit tests for the API layer without being asked
- Infrastructure code was idiomatic: parameterized Npgsql queries, no string concatenation, correct async/await patterns throughout

**Limitations observed:**
- Clean Architecture boundary violations were introduced once (BCrypt in Application) and required a correction pass
- The Angular post-login redirect was missing — the service saved state but didn't complete the navigation side effect
- Some issues only surfaced at runtime (port conflicts, seed hash mismatch), not at compile time — these required runtime debugging cycles

**Conclusion:** AI was used as a pair programmer, not a code generator. Every generated file was reviewed, several required targeted corrections, and all architectural decisions were driven by the developer's specification. The final codebase reflects deliberate engineering choices, not uncritical AI output.
