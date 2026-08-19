# TOOL_STRATEGY.md

## Feature Usage Log (Case Study)

### Entry 1
- **Feature Used:** Copilot Chat (Ask)
- **Why this feature:** Best for quickly generating initial module scaffolding from natural language requirements.
- **What happened:** Produced baseline transaction and expense model/service structure that accelerated implementation.

### Entry 2
- **Feature Used:** Inline Edit in editor
- **Why this feature:** Faster for local, line-level refactors than re-prompting whole files in chat.
- **What happened:** Used to tighten method signatures, improve naming, and adjust controller responses in-place.

### Entry 3
- **Feature Used:** Copilot Chat (iterative refinement)
- **Why this feature:** Useful for decomposing complex business rules into smaller validated steps.
- **What happened:** Refined split logic to separately handle equal and custom modes with explicit validation.

### Entry 4
- **Feature Used:** Copilot for test generation
- **Why this feature:** Efficient for creating repetitive test skeletons and scenario permutations.
- **What happened:** Generated xUnit test scaffolding that was then adjusted for deterministic assertions.

### Entry 5
- **Feature Used:** Copilot explanations / Q&A
- **Why this feature:** Helpful for quick clarification while integrating EF Core DbContext and DI registration.
- **What happened:** Confirmed missing wiring points and package needs during compile-fix loop.

### Entry 6
- **Feature Used:** Copilot-assisted documentation drafting
- **Why this feature:** Best for converting implementation activity into structured artifacts quickly.
- **What happened:** Drafted prompt chain documentation and collaboration notes, then manually corrected technical specifics.

### Entry 7
- **Feature Used:** (If used) Copilot PR/review assistant style prompts
- **Why this feature:** Useful to simulate review mindset and identify maintainability/security concerns.
- **What happened:** Helped formulate concrete peer-review style feedback and follow-up improvements.

---

## Scenario Responses

### 1) Understanding a complex 500-line function in an unfamiliar codebase
I would use **Copilot Chat (Ask)** with targeted prompts like “explain this function in phases” and “identify side effects and dependencies.” Chat is better than inline suggestions here because the goal is comprehension and risk mapping, not immediate code generation.

### 2) Adding consistent error handling across 8 existing route handlers
I would use **Inline Edit / multi-file edit support** (or Chat with explicit refactor instructions) to apply a repeated pattern. This is ideal for consistency because it can transform similar blocks quickly while preserving local context in each handler.

### 3) Quickly verifying a regex handles international phone number formats
I would use **Copilot Chat** to generate representative valid/invalid samples and explain regex coverage boundaries. Chat is effective for fast scenario-based validation and spotting blind spots before writing tests.

### 4) Enforcing automated code quality checks on every pull request with no human intervention
I would use **GitHub Actions** (workflow automation) and can use Copilot to draft the workflow YAML. The enforcement itself is CI policy, not just code suggestion, so workflow-based gates are the correct mechanism.

### 5) Reviewing a teammate's AI-generated authentication module for security vulnerabilities
I would use **PR review workflow + Copilot Chat** to inspect token validation, claim checks, secret handling, and authorization boundaries. This approach supports both line-level comments and higher-level threat-model reasoning.

### 6) Ensuring Copilot follows project-specific conventions consistently across all developers and sessions
I would use **repository-level Copilot custom instructions** (`.github/copilot-instructions.md`). This gives durable, shared constraints that guide output style and architecture conventions across contributors.

---

## Limitations Encountered

### Limitation 1 — Structural syntax error in generated DbContext
- **Prompt used:** Asked Copilot to integrate SharedExpense entities into DbContext.
- **What went wrong:** Generated/accepted structure placed DbSet declarations inside constructor scope.
- **How detected:** Build error and manual file inspection.
- **How fixed:** Moved DbSet properties to class scope and rebuilt.
- **What I’d do differently:** Prompt with explicit constraint: “DbSet properties must be class-level, not inside constructor.”

### Limitation 2 — Incomplete environment/package assumptions
- **Prompt used:** Generated transaction/expense wiring with EF Core + JWT references.
- **What went wrong:** Output assumed required NuGet packages were already installed.
- **How detected:** Namespace/type resolution compile errors.
- **How fixed:** Added missing package references and restored.
- **What I’d do differently:** Ask Copilot to include a package checklist with exact `dotnet add package` commands.

### Limitation 3 — Business-rule ambiguity in financial rounding/netting behavior
- **Prompt used:** Requested equal/custom split and net balance logic.
- **What went wrong:** Initial logic required manual validation of rounding behavior and sign semantics.
- **How detected:** Cross-checking expected balance direction and edge-case tests.
- **How fixed:** Clarified net amount convention and added/updated tests for opposite-direction aggregation and edge cases.
- **What I’d do differently:** Add explicit numeric examples in prompt (input/output) and require table-form expected results.