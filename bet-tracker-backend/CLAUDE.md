# BetTracker Backend — CLAUDE.md

Reference document for this repository. Read this before writing any code here.

## 1. What this is

Personal sports-bet and casino-session tracker. ASP.NET Core 8 Web API, EF Core + MySQL 8 (Pomelo provider),
JWT auth with refresh tokens, BCrypt password hashing. Frontend is a Vite app on
`http://localhost:5173` (CORS allowed origin).

## 2. Solution layout

```
bet-tracker-backend/
  BetTracker.sln
  BetTracker.Api/              HTTP boundary: controllers, middleware, validators, Program.cs
  BetTracker.Core/             Entities, enums, DTOs, service interfaces, domain logic (no EF, no ASP.NET)
  BetTracker.Infrastructure/   DbContext, EF configurations, seeders, service implementations
  BetTracker.Tests/            xUnit tests (profit calculator, bankroll recompute)
```

Reference direction (never reverse it):

```
Api  ->  Core
Api  ->  Infrastructure   (composition root only: DI registration)
Infrastructure -> Core
Core -> nothing
```

Core has zero dependencies on EF Core, ASP.NET Core or FluentValidation. That is what makes the
domain rules unit-testable without a database and what makes the "switch MySQL -> PostgreSQL"
change a single line in `Program.cs`.

## 3. Non-negotiable conventions

- **No comments in code.** Names carry the meaning. Explanation lives in this file / README.
- **Never expose entities.** Every request and response is a DTO in `BetTracker.Core/Dtos`.
- **User scoping in the service layer**, not only in controllers. Every query starts with
  `.Where(x => x.UserId == userId)`. Controllers pass the id, services enforce it.
- **UTC everywhere.** Store UTC, accept ISO 8601, never `DateTime.Now`. Time comes from `IClock`.
- **Decimal precision**: money `(18,2)`, odds and lines `(10,3)`.
- **Async all the way**: `Async` suffix, `CancellationToken` as last parameter.
- **One `SaveChangesAsync` per unit of work.** Multi-step writes that touch bankroll use an
  explicit `IDbContextTransaction`.
- Interfaces live in `Core/Abstractions`, implementations in `Infrastructure`.
- Errors are thrown as domain exceptions (`NotFoundException`, `ConflictException`,
  `ForbiddenException`, `DomainRuleException`) and translated to `ProblemDetails` by one
  middleware. Controllers contain no try/catch.

## 4. Domain rules (single source of truth: `Core/Domain/ProfitCalculator`)

Bankroll changes **only on settlement**, by exactly `Profit`. Stake is not deducted when a bet is
placed (it is reported separately as `amountAtRisk`).

| Status     | Profit                     |
|------------|----------------------------|
| Pending    | 0 (excluded from stats)    |
| Won        | `Stake * (Odds - 1)`       |
| Lost       | `-Stake`                   |
| Refunded   | `0`                        |
| CashedOut  | `CashoutAmount - Stake`    |

Editing or deleting a settled bet reverses the old `BankrollTransaction` and applies a new one
inside one DB transaction.

Parlays:
- `Odds` on the parent is the combined odds, defaulting to the product of leg odds, user-overridable.
- Void leg -> `EffectiveOdds = Odds / voidLeg.Odds` (a void leg collapses to 1.0), applied for each
  void leg, which preserves a user override proportionally.
- Parent status derives from legs: any `Lost` -> Lost; all non-void `Won` -> Won; all `Void` ->
  Refunded; otherwise Pending. The user may still settle the parent directly (e.g. CashedOut),
  which freezes leg editing.
- Settling a leg re-evaluates the parent and applies the bankroll transaction **exactly once**.

CLV: `(Odds / ClosingOdds - 1) * 100`, per bet and per leg; combined parlay CLV only when every leg
has `ClosingOdds`. CLV is analytics only, never touches profit or bankroll.

Units: `units = amount / user.UnitSize`, computed on read, never stored. Every stats response
returns currency **and** unit values.

## 5. Build order (one step at a time, each step ends with a working endpoint)

- [ ] 0. Solution scaffold, projects, package references
- [ ] 1. Register: User entity, DbContext, BCrypt hasher, ledger seed row, `POST /api/auth/register`
- [ ] 2. JWT + refresh tokens: login, refresh, logout, `GET /api/auth/me`, `ICurrentUser`
- [ ] 3. Reference data: Sport, BetCategory, CasinoGame + seeders + read endpoints
- [ ] 4. Custom sports/categories (`OwnerUserId` scoping) + write endpoints
- [ ] 5. Bets: entity, single-bet CRUD, ProfitCalculator, bankroll ledger writes
- [ ] 6. BetLegs, parlay settlement, leg settle endpoint
- [ ] 7. Filtering/paging/search on `GET /api/bets` (shared `BetQuery` reused by every stats endpoint)
- [ ] 8. Gambling sessions CRUD
- [ ] 9. Bankroll history, adjust, initial recompute
- [ ] 10. Stats endpoints (all consume the same filter object)
- [ ] 11. CLV, by-bet-type, loss-limit alerts
- [ ] 12. Import/Export
- [ ] 13. Tests + README

## 6. Reusability rules that matter later

- One `BetFilter` record + one `IQueryable<Bet> ApplyFilter(...)` extension is used by
  `GET /api/bets` **and** every `/api/stats/*` endpoint. Never duplicate filter logic.
- One `AuthResponse` shape is returned by register, login and refresh.
- One `PagedResult<T>` for every paged endpoint.
- One `MoneyValue { Currency, Units }` projection so stats never re-implement unit conversion.
- Seed data is idempotent and keyed by `Slug` so re-running never duplicates rows.

## 7. Commands

```bash
dotnet build
dotnet run --project BetTracker.Api
dotnet test
dotnet ef migrations add <Name> --project BetTracker.Infrastructure --startup-project BetTracker.Api
dotnet ef database update --project BetTracker.Infrastructure --startup-project BetTracker.Api
```

Swagger: `http://localhost:5000/swagger` (dev only).

## 8. Database

MySQL 8.0.46, local service `MySQL80` on `localhost:3306`. Provider: `Pomelo.EntityFrameworkCore.MySql`.

- Schema `bettracker`, charset `utf8mb4`, collation `utf8mb4_0900_ai_ci`.
- Application user `bettracker` — never connect as `root` from the app.
- The connection string (with the password) lives in **user-secrets**, not in `appsettings*.json`.
- `Database:Provider` in config selects the provider; the `switch` lives only in
  `Infrastructure/DependencyInjection.cs`.
- Server version is declared explicitly (`new MySqlServerVersion(new Version(8, 0, 46))`), not
  `ServerVersion.AutoDetect`, so `dotnet ef` and CI never need a reachable database to build.
- **Migrations are provider-specific.** Changing provider means deleting `Infrastructure/Migrations`
  and regenerating, never hand-editing.
- MySQL collation is case-insensitive by default, so email uniqueness and `search` `Contains`
  filters are case-insensitive at the database level. Emails are still normalized to lowercase on
  write so behaviour does not depend on the server's collation.
- `decimal(18,2)` and `decimal(10,3)` are real types here (SQLite would have stored them as REAL),
  so money arithmetic is exact.
