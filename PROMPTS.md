# PROMPTS.md — Expense Splitting Feature (Part 3D)

## Overview
This document records the exact prompt chain I used with GitHub Copilot while implementing the Expense Splitting feature (Part 3C), including feature mode used, prompting technique, and rationale.

---

## Prompt Chain (Execution Order)

### Prompt 1 — Domain model scaffolding
- **Copilot Feature Used:** Chat (Ask mode)
- **Exact Prompt Text:**
  > Generate C# EF Core models for an expense splitting module:
  > 1) SharedExpense (Id, CreatorUserId, Description, TotalAmount, SplitType, CreatedAtUtc)
  > 2) SharedExpenseParticipant (Id, SharedExpenseId, ParticipantUserId, ShareAmount)
  > Include navigation properties and suitable data annotations.
- **Prompting Technique(s):** Specificity, Constraint
- **Why I used this approach:**
  - I specified exact fields and expected ORM details to avoid vague output.
  - I constrained output to EF Core-ready model design.

---

### Prompt 2 — Service decomposition and business rules
- **Copilot Feature Used:** Chat (Ask mode)
- **Exact Prompt Text:**
  > Create an `IExpenseSplittingService` and `ExpenseSplittingService` with:
  > - create shared expense
  > - equal split logic
  > - custom split logic
  > - validation that custom sum equals total
  > - pending net balances per current user
  > Use clean layered architecture and throw validation exceptions for bad input.
- **Prompting Technique(s):** Decomposition, Specificity
- **Why I used this approach:**
  - I decomposed the feature into explicit sub-functions so Copilot produced structured logic instead of a monolith.
  - I explicitly defined validation expectations.

---

### Prompt 3 — Controller/API contract generation
- **Copilot Feature Used:** Inline Edit (Editor)
- **Exact Prompt Text:**
  > Generate `ExpensesController` endpoints:
  > POST /api/expenses/shared
  > GET /api/expenses/balances/pending
  > Must be [Authorize], extract current user from claims, call service only (no business logic in controller), return proper status codes.
- **Prompting Technique(s):** Constraint, Role-based
- **Why I used this approach:**
  - I constrained controller responsibilities to orchestration only.
  - I used a role-based framing (“controller only”) to enforce layered boundaries.

---

### Prompt 4 — DI and DbContext integration
- **Copilot Feature Used:** Chat (Ask mode)
- **Exact Prompt Text:**
  > Update AppDbContext and Program.cs to register SharedExpense entities and expense services.
  > Add DbSet and entity configurations for SharedExpense and SharedExpenseParticipant.
  > Add dependency injection wiring for repository and service.
- **Prompting Technique(s):** Iterative Refinement, Specificity
- **Why I used this approach:**
  - After generating module files, I iteratively asked for integration wiring.
  - This reduced missed dependencies and compile-time issues.

---

### Prompt 5 — Test generation with rubric alignment
- **Copilot Feature Used:** Chat (Ask mode)
- **Exact Prompt Text:**
  > Generate xUnit tests for ExpenseSplittingService covering:
  > 1. equal split among 3 users
  > 2. valid custom split
  > 3. invalid custom split total
  > 4. opposite-direction net balance aggregation
  > 5. single participant edge case
  > 6. unauthorized access case
  > Keep tests deterministic and simple.
- **Prompting Technique(s):** Few-shot-by-enumeration, Constraint, Decomposition
- **Why I used this approach:**
  - I enumerated expected test scenarios directly from rubric requirements.
  - This constrained Copilot to required acceptance coverage.

---

## Copilot Features Used (Requirement Check)
At least 2 features were used:
1. **Chat (Ask mode)**
2. **Inline Edit (Editor)**
3. **Error Fix (Agent mode)**

---

## Prompting Techniques Used (Requirement Check)
At least 3 techniques were used:
1. **Specificity**
2. **Decomposition**
3. **Constraint**
4. **Role-based prompting**
5. **Iterative refinement**
6. **Few-shot-by-enumeration**

---

## Post-Generation Corrections

### Correction 1 — DbSet placement bug in AppDbContext
- **What was wrong:** `DbSet<SharedExpense>` and `DbSet<SharedExpenseParticipant>` were mistakenly placed inside the constructor block.
- **Why it was wrong:** C# syntax and EF Core conventions require DbSet declarations at class scope.
- **Fix applied:** Moved both DbSet properties outside constructor to class-level properties.

### Correction 2 — Project structure mismatch
- **What was wrong:** Initial generated files existed under `src/transactions` outside the actual Web API project.
- **Why it was wrong:** Files outside `src/FinTrack.Api` were not included in project compilation.
- **Fix applied:** Moved active feature code under `src/FinTrack.Api/...`; archived initial AI output in `evidence/part2-initial-ai-output`.

### Correction 3 — Missing package references
- **What was wrong:** Build errors for EF Core, Identity EF Core, JWT namespaces.
- **Why it was wrong:** Required NuGet packages were not yet installed in `FinTrack.Api.csproj`.
- **Fix applied:** Added missing packages (`Microsoft.EntityFrameworkCore`, `SqlServer`, `Identity.EntityFrameworkCore`, `JwtBearer`, etc.) and rebuilt.

### Correction 4 — Architecture boundary tightening
- **What was wrong:** Risk of business rules creeping into controller flow.
- **Why it was wrong:** Violates layered design and makes testing harder.
- **Fix applied:** Kept validation/splitting/netting logic in service layer; controller now only handles auth context + HTTP responses.

### Correction 5 — Traceability for assessment evidence
- **What was wrong:** Original unreviewed AI-generated Part 2 files could be confused with remediated files.
- **Why it was wrong:** Reduces clarity for reviewers.
- **Fix applied:** Archived original generated artifacts under `evidence/part2-initial-ai-output/` and kept remediated code in active module paths.

---

## Final Outcome
Using structured prompt chaining with Copilot, I implemented and validated the Expense Splitting feature with:
- layered architecture,
- validated split logic (equal/custom),
- pending net balance calculation,
- authenticated endpoints,
- and passing automated tests.