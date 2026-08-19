# ARCHITECTURE.md

The system is organized into two domain modules: **Transactions** and **Expense Splitting**.  
Transactions manage individual user-owned financial records (create/list/delete scoped to authenticated user).  
Expense Splitting builds on top of that foundation to model shared expenses and participant-level obligations.  
Both modules follow a layered architecture: **Controller → Service → Repository → DbContext/Database**.  
Controllers handle HTTP concerns, auth context, and response codes only.  
Services contain business rules (validation, split logic, net balance computation, ownership checks).  
Repositories isolate EF Core persistence and query concerns from business logic.  
Data flow starts at authenticated API requests, passes through validation/rules, then persists and returns normalized DTO responses.  
SharedExpense and SharedExpenseParticipant entities capture split metadata and per-user share amounts for balance calculations.  
Net balances are computed from stored shared expenses to show who owes whom for the current user.  
This architecture is suitable for fintech because it enforces separation of concerns, auditability, and safer change management.  
Key design decisions: strict user-scoped authorization, explicit validation exceptions, structured logging, and deterministic unit tests.  
I also preserved initial AI outputs in evidence/archive paths to maintain traceability during remediation.