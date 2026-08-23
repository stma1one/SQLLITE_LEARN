# Phase Status Templates

## Baton Stage: Planning Coach

```text
PHASE STATUS
============
Mode: MAXIMIZE STUDENT CAPABILITY
Complete: no
Workflow Stage: PLANNING_COACH
Status: PLANNING_WITH_STUDENT

What we are deciding now:
- ...

Question 1:
...

Question 2:
...

NEXT:
Answer these planning questions. After that I will summarize the plan for approval.
```

## Baton Stage: Plan Confirmation

```text
PHASE STATUS
============
Mode: MAXIMIZE STUDENT CAPABILITY
Complete: no
Workflow Stage: PLAN_CONFIRMATION
Status: PLAN_APPROVAL_PENDING

Proposed plan:
- Goal: ...
- User flow: ...
- Data/state: ...
- Files likely affected: ...
- First small implementation slice: ...
- Out of scope for now: ...

NEXT:
Approve this plan or correct it before we move to the concept lesson.
```

## Baton Stage: Concept Tutor

```text
PHASE STATUS
============
Mode: MAXIMIZE STUDENT CAPABILITY
Complete: no
Workflow Stage: CONCEPT_TUTOR
Status: CONCEPT_GATE_PENDING

Concept for this slice:
...

Why it matters here:
...

Concept weight:
- Crucial / Medium / Routine: ...

Understanding questions:
1. ...
2. ...

NEXT:
Answer in your own words. "Approve" or "continue" is not enough for an explanation question.
```

## Baton Stage: Stuck Student Support

Use this when the student says "I do not know", "continue", "do it", "my teacher said it is fine", or gives another bypass answer during a concept gate.

```text
PHASE STATUS
============
Mode: MAXIMIZE STUDENT CAPABILITY
Complete: no
Workflow Stage: CONCEPT_TUTOR
Status: CONCEPT_GATE_PENDING

What seems blocked:
- ...

Small hint or quick lesson:
- ...

Smaller checkpoint:
...

NEXT:
Answer the smaller checkpoint. If it is still unclear, say which word or step is confusing.
```

## Baton Stage: Ownership Negotiator

```text
PHASE STATUS
============
Mode: MAXIMIZE STUDENT CAPABILITY
Complete: no
Workflow Stage: OWNERSHIP_NEGOTIATOR
Status: OWNERSHIP_SELECTION_PENDING

Planned changes:
- Good student practice: ...
- Agent can scaffold: ...
- Agent-safe routine work: ...
- Shared review: ...
- Testing plan: ...

Choose one:
1. Student implements the core logic with agent scaffold, hints, and review.
2. Student implements selected core parts; agent handles approved routine wiring.
3. Agent implements only routine or already-mastered parts; student explains and approves the test plan.

NEXT:
Tell me which option you choose and, if needed, which exact items belong to you.
```

## Baton Stage: Execution Hand-Off

Use this only after plan approval, concept-gate evidence, and ownership selection.

```text
PHASE STATUS
============
Mode: MAXIMIZE STUDENT CAPABILITY
Complete: no
Workflow Stage: EXECUTION_AGENT
Status: WAITING_FOR_STUDENT

What the agent changed:
- ...

Why these files:
- ...

Student TODO:
- File: ...
- Task: ...
- Constraints: ...

NEXT:
Implement the TODO, then tell me what you changed and why.
```

## Baton Stage: Verification

```text
PHASE STATUS
============
Mode: MAXIMIZE STUDENT CAPABILITY
Complete: yes/no
Workflow Stage: VERIFICATION_AUDITOR
Status: COMPLETE / READY_TO_VERIFY / BLOCKED

Evidence:
- Build: ...
- Tests: ...
- Scripts: ...
- Manual checks: ...

Review:
- What worked: ...
- What needs correction: ...

NEXT:
...
```
