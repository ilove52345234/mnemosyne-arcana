# 02 - Design Doc Coverage Matrix

## 1. 狀態定義
- Not Started：尚未建立對應驗測案例
- In Progress：已有案例但未完成 3+ 模型覆蓋
- Covered：已完成 3+ 模型覆蓋且有證據

## 2. 覆蓋矩陣
| 設計文件 | 對應系統 | 驗測要求 | 目前狀態 | 證據/輸出 |
|---|---|---|---|---|
| `docs/01-game-design-core.md` | S1/S2/S3/S5/S7 | 每系統 3+ 模型 | In Progress | 現有 EditMode + Prototype logs |
| `docs/02-meta-progression.md` | S5/S6/S7 | 每系統 3+ 模型 | In Progress | Meta/Contract tests |
| `docs/03-technical-architecture.md` | S1~S6 | 模組邊界與流程覆蓋 | In Progress | Manager tests + flow logs |
| `docs/09-nfr-and-quality-gates.md` | S8/S9 | NFR 三模型壓測 | Not Started | 待建壓測報告 |
| `docs/14-core-usecase-sequences.md` | S1/S5/S6 | Use case 序列三模型 | In Progress | UserStoryAcceptanceTests |
| `docs/15-balance-source-of-truth.md` | S2/S5/S7 | 平衡三模型 + 多 seed | In Progress | 10 模型/30 輪結果 |
| `docs/17-test-matrix.md` | S1~S9 | 測試 ID 對齊與補齊 | In Progress | 現有 test matrix |
| `docs/23-user-stories-and-use-cases.md` | S1/S3/S5/S6 | US/UC 覆蓋三模型 | In Progress | US01~US12 測試 |
| `docs/24-vocab-growth-curve-and-gating-plan.md` | S4/S7/S8 | 十模型 + 長週期遺忘 | In Progress | `docs/25` + MCP logs + `S4PriorityValidationTests` |
| `docs/25-gate-model-sweep-report-2026-02-17.md` | S4/S7 | 30 輪分佈 | Covered | M9 12/30(40%) |

## 3. 缺口清單（需優先補齊）
1. `S9 NFR`：尚未有低/中/高負載三模型壓測結果。
2. `S4 長週期遺忘`：7/14/30 天退回機制需完整分佈報告。
3. `S7 Final/Endless`：95%/100%+7天 + 無盡模式需三模型覆蓋。

## 4. 執行順序（建議）
1. S4 長週期遺忘與退回（高優先）
2. S7 Final Gate + Endless（高優先）
3. S9 NFR 壓測（中優先）
4. 其餘文件補齊 Covered 狀態（中優先）
