# 02 - Design Doc Coverage Matrix

## 1. 狀態定義
- Not Started：尚未建立對應驗測案例
- In Progress：已有案例但未完成 3+ 模型覆蓋
- Covered：已完成 3+ 模型覆蓋且有證據

## 2. 覆蓋矩陣
| 設計文件 | 對應系統 | 驗測要求 | 目前狀態 | 證據/輸出 |
|---|---|---|---|---|
| `docs/01-game-design-core.md` | S1/S2/S3/S5/S7 | 每系統 3+ 模型 | Covered | `RunFlowTests` + `ScoringHandTypeTests` + `LearningManagerTests` + `ShopManagerTests` + `S7FinalGateValidationTests` + MCP job `7077ee7ea9df451887a88308342a0093` |
| `docs/02-meta-progression.md` | S5/S6/S7 | 每系統 3+ 模型 | Covered | `ShopManagerTests` + `MetaManagerTests` + `S7FinalGateValidationTests` + MCP job `7077ee7ea9df451887a88308342a0093` |
| `docs/03-technical-architecture.md` | S1~S6 | 模組邊界與流程覆蓋 | Covered | `PlayableLoopUseCaseTests` + `UserStoryAcceptanceTests` + Core manager tests + MCP job `7077ee7ea9df451887a88308342a0093` |
| `docs/09-nfr-and-quality-gates.md` | S8/S9 | NFR 三模型壓測 | Covered | `S8TelemetryModelCoverageTests` + `S9NfrValidationTests` + MCP EditMode job `7077ee7ea9df451887a88308342a0093` |
| `docs/14-core-usecase-sequences.md` | S1/S5/S6 | Use case 序列三模型 | Covered | `PlayableLoopUseCaseTests` + `UserStoryAcceptanceTests` + MCP job `7077ee7ea9df451887a88308342a0093` |
| `docs/15-balance-source-of-truth.md` | S2/S5/S7 | 平衡三模型 + 多 seed | Covered | `docs/25` + `GateModelSweepTests` + `S7FinalGateValidationTests` |
| `docs/17-test-matrix.md` | S1~S9 | 測試 ID 對齊與補齊 | Covered | `TC-S4/S7/S8/S9` 已回填，EditMode `133/133` 通過（job `7077ee7ea9df451887a88308342a0093`） |
| `docs/23-user-stories-and-use-cases.md` | S1/S3/S5/S6 | US/UC 覆蓋三模型 | Covered | `UserStoryAcceptanceTests`（US01~US12）+ `PlayableLoopUseCaseTests` + MCP job `7077ee7ea9df451887a88308342a0093` |
| `docs/24-vocab-growth-curve-and-gating-plan.md` | S4/S7/S8 | 十模型 + 長週期遺忘 | Covered | `docs/25` + `S4PriorityValidationTests` + `S4LongCycleDistributionTests` + `S7FinalGateValidationTests` + `S8TelemetryModelCoverageTests` |
| `docs/25-gate-model-sweep-report-2026-02-17.md` | S4/S7 | 30 輪分佈 | Covered | M9 12/30(40%) |

## 3. 缺口清單（需優先補齊）
1. 需維持 nightly regression 與新增需求同步回填，避免 `Covered` 漂移。

## 4. 執行順序（建議）
1. 完成最終 sign-off 報告與 Go/No-Go 判定（中優先）
2. 轉入 A-02（存檔/migration）驗測（高優先）

## 5. 行為驗測追蹤（2026-02-18 Replan）

> 說明：本區塊只追蹤「實際遊戲行為是否符合設計」，不以單元測試通過取代。

| 系統 ID | 系統名稱 | 三模型行為驗測 | 狀態 |
|---|---|---|---|
| GS-01 | Run/Blind 推進 | M-Low/M-Mid/M-High | Pending |
| GS-02 | 牌型/得分（含同族） | M-Low/M-Mid/M-High | Pending |
| GS-03 | 學習題型/答錯三選一 | M-Low/M-Mid/M-High | Pending |
| GS-04 | Boss 學習機制 | M-Low/M-Mid/M-High | Pending |
| GS-05 | 商店/經濟/重擲 | M-Low/M-Mid/M-High | Pending |
| GS-06 | Build 五層交互 | M-Low/M-Mid/M-High | Pending |
| GS-07 | Gate/Recovery/Demotion | M-Low/M-Mid/M-High(+M-Edge) | Pending |
| GS-08 | Final Gate/Endless | M-Low/M-Mid/M-High(+M-Edge) | Pending |
| GS-09 | Meta/Contract/Curriculum | M-Low/M-Mid/M-High | Pending |
| GS-10 | 詞庫演進/五池抽樣 | M-Low/M-Mid/M-High | Pending |
| GS-11 | Telemetry/告警 | M-Low/M-Mid/M-High | Pending |
| GS-12 | NFR 行為穩定 | M-Low/M-Mid/M-High(+M-Edge) | Pending |
| GS-13 | 遺物系統（Legacy） | 先裁決 Retain/Retire | Pending |
| GS-14 | 卡牌進化（Legacy） | 先裁決 Retain/Retire | Pending |
| GS-15 | 舊書房系統（Legacy） | 先裁決 Retain/Retire | Pending |
