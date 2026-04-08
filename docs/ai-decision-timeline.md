# AI Decision Timeline

## Purpose

This document captures **non-trivial AI-assisted decisions** so that:

- Mistakes are not rediscovered
- Framework invariants remain visible
- AI tools are guided by historical context

This is not a chat log.
This is a **governance artifact**.

---

## When to Add an Entry

Add an entry when:

- AI produced plausible but incorrect output
- A framework invariant was rediscovered
- A fix required multiple AI iterations
- The outcome should influence future AI behavior
- A refusal based on architectural or framework constraints
- A correction after multiple iterations
- Identification of an incorrect assumption

The AI MUST:

1. Propose a new entry for `docs/ai-decision-timeline.md`
2. Ask for confirmation before writing
3. Use the canonical entry format

Failure to do so is a governance violation.

Do NOT add entries for trivial fixes.

---

## Entry Format

### YYYY-MM-DD — Short Description

**Context**  
What was being built or fixed.

**Observed Failure**  
What went wrong (symptoms).

**Incorrect Assumption**  
What assumption (human or AI) proved false.

**Root Cause**  
The actual invariant or rule that was violated.

**Correct Pattern**  
The canonical approach that resolved the issue.

**Enforcement Update**  
What rule, instruction, or prompt was updated to prevent recurrence.

---

## Current Status

_No deviations recorded._

The absence of entries indicates full canonical compliance.
