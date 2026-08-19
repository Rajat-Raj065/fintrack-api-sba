# PR Description — FinTrack API Case Study

## Summary
This PR delivers a production-oriented remediation and extension of AI-generated backend code.

### What was built
1. **Transaction module remediation (Part 3B)**
   - Refactored into layered architecture:
     - models
     - repositories
     - services
     - controllers
   - Added validation, structured logging, and authenticated user ownership checks.
   - Integrated DbContext and service registration for runtime consistency.

2. **Expense Splitting feature (Part 3C)**
   - Added `SharedExpense` and `SharedExpenseParticipant` domain models.
   - Implemented split logic:
     - equal split
     - custom split
     - custom sum validation
   - Implemented pending net balance calculation per current user.
   - Added authenticated endpoints for create and balance retrieval.

3. **Testing**
   - Added automated xUnit tests for required functional and edge-case scenarios.
   - Current result: all tests passing locally.

## Why
The goal was to convert raw AI-generated scaffolding into maintainable, secure, and testable backend functionality suitable for team review and iterative delivery.

---

## AI Tool Disclosure

### Copilot features used
- Copilot Chat (Ask mode)
- Inline Edit (Editor completions/refactoring)
- (Optional, if used) PR/Review assistance for comment drafting

### Where AI output was accepted
- Initial model/repository/service/controller scaffolding
- Test skeletons and repetitive boilerplate

### Where AI output was overridden
- DbContext corrections (class-level DbSet placement)
- Validation and authorization tightening
- Net balance sign conventions and edge-case behavior
- Project structure alignment to compile within active `.csproj`

### Estimated contribution split
- **AI-generated:** ~65%
- **Hand-written / manually corrected:** ~35%

---

## Testing Coverage

### Covered
- Equal split among 3 participants
- Valid custom split
- Invalid custom split total
- Net balance aggregation in opposite directions
- Single participant edge case
- Unauthorized access case
- Additional positive-path validation (total 7 tests passing)

### Known gaps
- No full integration tests against real DB provider
- No performance/load tests for large expense histories
- No end-to-end auth token lifecycle tests

---

## Risks / Trade-offs
1. **Trade-off: Simplicity vs financial precision**
   - Current implementation uses decimal rounding at 2 precision points.
   - This is practical, but cumulative rounding behavior may require stricter reconciliation logic in high-volume production finance scenarios.

---

## Self-Review Checklist
- [x] Build succeeds from solution root
- [x] Tests pass (`dotnet test`)
- [x] Layered architecture preserved (controller thin, service logic centralized)
- [x] Auth checks enforce user-scoped access
- [x] Validation paths return clear errors
- [x] Documentation files updated (`REVIEW.md`, `PROMPTS.md`, `PR_DESCRIPTION.md`, `TOOL_STRATEGY.md`)
- [x] Archived initial AI-generated artifacts retained for traceability

---

## Peer Review Simulation

### Comment 1 — Actionable architecture note
**Location:** `src/FinTrack.Api/expenses/services/ExpenseSplittingService.cs` (`CreateSharedExpenseAsync`)  
**Comment:** Consider extracting split computation (`equal` vs `custom`) into private strategy methods or separate strategy classes.  
**Why:** This will reduce method complexity and make rule-specific unit testing easier as split rules evolve.

### Comment 2 — Actionable API contract note
**Location:** `src/FinTrack.Api/expenses/controllers/ExpensesController.cs` (`CreateSharedExpense`)  
**Comment:** Return a resource location via `CreatedAtAction(...)` instead of `Created(string.Empty, ...)`, and add a GET-by-id endpoint for shared expenses.  
**Why:** Improves REST consistency and makes created resource retrieval explicit for API consumers.

### Comment 3 — AI-miss category (domain risk)
**Location:** `src/FinTrack.Api/expenses/services/ExpenseSplittingService.cs` (rounding logic and sum equality checks)  
**Comment:** Add explicit invariant tests for rounding drift scenarios (e.g., 100 split among 3, mixed custom cents) and document reconciliation policy.  
**Why:** AI-generated financial logic often appears correct on happy paths but can miss small precision edge cases that create accounting mismatches over time.