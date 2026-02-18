# Alpha Gap Closure Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Close the remaining implementation gaps so Mnemosyne Arcana can satisfy full interactive design scope and clear Alpha exit criteria.

**Architecture:** Keep domain rules in `Core/Managers` and isolate persistence/telemetry behind services. Use prototype UI as the integration surface while progressively replacing placeholder logic with design-compliant flows.

**Tech Stack:** Unity 2022.3 LTS, C#, Unity EditMode tests, JSON config + schemas, shell/python validation scripts.

---

### Task 1: A-02 Save/Migration Foundation

**Files:**
- Create: `Assets/MnemosyneArcana/Scripts/Core/Persistence/SaveServiceV2.cs`
- Create: `Assets/MnemosyneArcana/Scripts/Core/Persistence/MigrationServiceV1ToV2.cs`
- Create: `Assets/MnemosyneArcana/Tests/EditMode/SaveMigrationTests.cs`
- Modify: `Assets/MnemosyneArcana/Scripts/Core/Contracts/ServiceInterfaces.cs`
- Modify: `Assets/MnemosyneArcana/Scripts/Core/Managers/MetaManagerV2.cs`

**Steps:**
1. Add failing tests for save/load, migration success, migration rollback.
2. Implement `SaveServiceV2` with backup-before-migrate flow.
3. Implement v1->v2 field mapping and strict error reporting.
4. Wire service into `MetaManagerV2` APIs for persistence lifecycle.
5. Run EditMode tests + config validation.

### Task 2: A-03 Performance/Soak Verification Harness

**Files:**
- Create: `Assets/MnemosyneArcana/Tests/EditMode/PerformanceSoakTests.cs`
- Create: `scripts/run_alpha_soak.sh`
- Modify: `docs/baseline/09-nfr-and-quality-gates.md`
- Modify: `docs/baseline/22-alpha-a01-regression-checklist.md`

**Steps:**
1. Add failing test scaffold for deterministic multi-run loops and allocation guardrails.
2. Add CLI script to run repeated EditMode test batches and collect logs.
3. Define pass/fail thresholds mapped to NFR table.
4. Run soak command and attach result summary in docs.

### Task 3: Core Loop Rule-Compliance Gap (Run + Learning)

**Files:**
- Modify: `Assets/MnemosyneArcana/Scripts/Core/Managers/RunManagerV2.cs`
- Modify: `Assets/MnemosyneArcana/Scripts/Core/Managers/LearningManagerV2.cs`
- Create: `Assets/MnemosyneArcana/Tests/EditMode/RunRulesComplianceTests.cs`
- Create: `Assets/MnemosyneArcana/Tests/EditMode/LearningSafeguardTests.cs`

**Steps:**
1. Add failing tests for discard flow, hand-size/plays constraints, and blind modifiers.
2. Add wrong-answer safeguard logic (3/5 streak fallback behavior).
3. Implement runtime state fields/events required by docs.
4. Ensure existing run flow tests still pass.

### Task 4: Shop/Build Full Rule-Compliance Gap

**Files:**
- Modify: `Assets/MnemosyneArcana/Scripts/Core/Managers/ShopManagerV2.cs`
- Create: `Assets/MnemosyneArcana/Tests/EditMode/ShopBuildRulesTests.cs`
- Modify: `docs/baseline/01-game-design-core.md`
- Modify: `docs/baseline/15-balance-source-of-truth.md`

**Steps:**
1. Add failing tests for reroll cost progression, nurture slots, pack slot behavior.
2. Implement missing shop slot types and reroll economy.
3. Keep Ante-segment weighting and Boss course behavior intact.
4. Validate all shop rules against SoT.

### Task 5: Meta Scope Closure (Course Tree + Lexicon Evolution)

**Files:**
- Modify: `Assets/MnemosyneArcana/Scripts/Core/Managers/MetaManagerV2.cs`
- Create: `Assets/MnemosyneArcana/Tests/EditMode/MetaProgressionFullTreeTests.cs`
- Create: `Assets/MnemosyneArcana/Tests/EditMode/LexiconEvolutionTests.cs`
- Modify: `configs/meta_progress.v2.json`

**Steps:**
1. Add failing tests for 4x12 curriculum constraints and lexicon tier unlock gates.
2. Expand node definitions from MVP subset to full spec scope.
3. Implement lexicon pool evolution heuristics with deterministic seed behavior.
4. Verify contract cap remains <=45% of total LP.

### Task 6: Runtime Eventing + UI Integration Hardening

**Files:**
- Modify: `Assets/MnemosyneArcana/Scripts/Core/Runtime/RuntimeContracts.cs`
- Create: `Assets/MnemosyneArcana/Scripts/Core/Runtime/EventBus.cs`
- Modify: `Assets/MnemosyneArcana/Scripts/Prototype/PrototypeCardGameUiController.cs`
- Create: `Assets/MnemosyneArcana/Tests/EditMode/RuntimeEventContractTests.cs`

**Steps:**
1. Add failing tests for required runtime events and payload schema versions.
2. Implement minimal event bus and emit key lifecycle events.
3. Bridge prototype UI to event consumption instead of direct state assumptions.
4. Validate no cyclic event loops.

### Task 7: Release Readiness Closure (A-04)

**Files:**
- Modify: `docs/IMPLEMENTATION_STATUS.md`
- Modify: `docs/PROJECT_EXECUTION_PLAN.md`
- Modify: `docs/SESSION_NOTES.md`
- Create: `docs/alpha-release-decision-record.md`

**Steps:**
1. Add release checklist entries for A-02/A-03 evidence.
2. Collect test artifacts and known-risk signoff.
3. Produce go/no-go decision record with blockers and mitigations.

