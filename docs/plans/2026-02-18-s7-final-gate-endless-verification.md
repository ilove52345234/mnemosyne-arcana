# S7 Final Gate/Endless Verification Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Complete S7 verification evidence with model coverage (M-Low/M-Mid/M-High/M-Edge), multi-seed runs, and endless stability checks, then apply minimal tuning if metrics miss target bands.

**Architecture:** Keep core gate thresholds (`95%`, `100%+7d`) in `GateProgressionManagerV2` as source of truth. Add deterministic EditMode verification harnesses for S7 model sweeps and endless long-run stability. Tune only prototype-side model parameters if S7 metrics drift from expected behavior.

**Tech Stack:** Unity 2022.3 EditMode tests (NUnit), Core managers, prototype simulation logic, docs in `docs/verification/*`.

---

### Task 1: Add S7 model coverage tests

**Files:**
- Create: `Assets/MnemosyneArcana/Tests/EditMode/S7FinalGateValidationTests.cs`
- Modify: `docs/17-test-matrix.md`

Steps:
1. Write failing tests for `S7-M1/M2/M3` using `EvaluateFinalMasteryGate`.
2. Run targeted test to confirm fail first.
3. Implement minimal assertions/data for pass criteria.
4. Run targeted + full EditMode tests.

### Task 2: Add S7-M4 endless stability test harness

**Files:**
- Modify: `Assets/MnemosyneArcana/Tests/EditMode/S7FinalGateValidationTests.cs`

Steps:
1. Add failing test for deterministic 30-seed endless stability simulation (no crash/invalid state, expected clear-band).
2. Run targeted test to confirm fail first.
3. Implement minimal deterministic simulation helper and assertions.
4. Run targeted + full EditMode tests.

### Task 3: Tune only if out-of-band

**Files:**
- Modify (if needed): `Assets/MnemosyneArcana/Scripts/Prototype/PrototypeCardGameUiController.cs`

Steps:
1. Evaluate S7 batch metrics from tests.
2. If out-of-band, adjust only S7-relevant prototype params with smallest delta.
3. Re-run S7 targeted + full EditMode tests.

### Task 4: Fill verification evidence

**Files:**
- Modify: `docs/verification/02-design-doc-coverage-matrix.md`
- Modify: `docs/verification/03-final-verification-report-template.md`
- Modify: `docs/25-gate-model-sweep-report-2026-02-17.md`
- Modify: `docs/SESSION_NOTES.md`

Steps:
1. Record S7 model coverage and job IDs/evidence.
2. Mark S7 gap as closed in coverage matrix.
3. Backfill final verification template with S7 section snapshot.
4. Add session handoff entry with outcomes and remaining gaps.
