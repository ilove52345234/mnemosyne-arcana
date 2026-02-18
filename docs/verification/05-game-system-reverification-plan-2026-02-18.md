# 05 - Game System Re-Verification Plan (Behavior First)

> Date: 2026-02-18  
> Scope: 全遊戲系統重新驗測（以實際行為為主，不以 code-level 通過視為完成）

## 1. 目標

1. 完整覆蓋 `docs/` 現行規格中的每個「遊戲系統」。
2. 每個系統都用同一套三模型（M-Low / M-Mid / M-High）驗測。
3. 核心高風險系統加跑 M-Edge。
4. 驗證基準改為「系統行為是否符合設計」，不是「測試碼是否通過」。

## 2. 驗測原則（這次強制）

1. 先跑行為場景，再看單元測試；單元測試只當輔助證據。
2. 每個系統至少 3 seeds；S4/S7 與經濟相關至少 30 seeds。
3. 每個結論必須有三種證據：
- PlayMode/MCP 操作軌跡
- 指標或狀態輸出（log/snapshot）
- 文件回填（矩陣+報告）
4. 不接受「只驗證 happy path」。每系統至少 1 個失敗/邊界場景。

## 3. 三模型定義（統一套用）

- `M-Low`：低掌握/低資源/高失誤，模擬新手與高遺忘。
- `M-Mid`：中掌握/中資源，模擬一般玩家。
- `M-High`：高掌握/高資源，模擬熟練玩家。
- `M-Edge`（僅核心系統）：極端長局、極端詞條、連敗連勝切換。

## 4. 全系統清單與目前狀態

| 系統 ID | 系統名稱 | 來源文件 | 目前狀態 | 本輪動作 |
|---|---|---|---|---|
| GS-01 | Run/Blind 推進 | `docs/01` `docs/14` | 有測試證據，行為驗測需補強 | 重跑三模型行為流 |
| GS-02 | 牌型/得分（含同族對/同族三/同族花/語序同族） | `docs/01` `docs/15` | 主要是 code 證據 | 補三模型實戰分布 |
| GS-03 | 學習題型/答錯三選一/保底 | `docs/01` | 有測試證據 | 補行為壓測與邊界 |
| GS-04 | Boss 學習機制 | `docs/01` | 有測試證據 | 補連對/全對實戰鏈 |
| GS-05 | 商店/經濟/重擲 | `docs/01` `docs/15` | 有測試證據 | 補低資金與高資金路徑 |
| GS-06 | Build 五層（語感/教材/詞綴/課程/頓悟） | `docs/01` | 行為證據不足 | 逐層驗測+交互驗測 |
| GS-07 | Gate/Recovery/Demotion | `docs/24` `docs/25` | 已有部分批次 | 以三模型重跑並記錄恢復率 |
| GS-08 | Final Gate/Endless | `docs/24` `docs/25` | 已有部分批次 | 補行為證據鏈一致性 |
| GS-09 | Meta/Contract/Curriculum | `docs/02` | 測試存在 | 補實際解鎖與互斥行為 |
| GS-10 | 詞庫演進/五池抽樣 | `docs/02` | 行為驗測不足 | 補長週期抽樣觀測 |
| GS-11 | Telemetry/告警 | `docs/09` `docs/24` | 有規則測試 | 補場景觸發一致性 |
| GS-12 | NFR 穩定性 | `docs/09` | 已有測試 | 補行為場景下資源曲線 |

## 5. 驗測分期

### Phase A：現行主規格全覆蓋（GS-01 ~ GS-12）

1. 每系統跑 M-Low/M-Mid/M-High。
2. GS-07/08/12 追加 M-Edge。
3. 每系統輸出：
- 行為腳本（操作步驟）
- 觀測指標
- Pass/Fail 與偏差說明

## 6. 每系統最小驗測模板

每個 GS 都必須至少包含：

1. `M-Low`：可玩性與保底是否成立。
2. `M-Mid`：是否落在設計期望帶。
3. `M-High`：上限行為是否合理，是否失衡。
4. `Failure Case`：至少 1 個失敗/邊界流程。
5. `Evidence`：操作紀錄 + 狀態快照 + 指標摘要。

## 7. 驗收門檻（本計劃）

1. `GS-01 ~ GS-12` 全部完成三模型行為驗測。
2. 覆蓋矩陣新增「Behavior Verified」欄位，未達成不得標記完成。
3. 最終報告需區分：
- Code-level pass
- Behavior-level pass

## 8. 交付文件

1. 本計劃：`docs/verification/05-game-system-reverification-plan-2026-02-18.md`
2. 執行矩陣：更新 `docs/verification/02-design-doc-coverage-matrix.md`（加 Behavior 欄）
3. 最終報告：新增行為驗測版報告（日期版）
4. Session 證據：`docs/SESSION_NOTES.md`

## 9. 立即執行順序（S7 優先後續）

1. GS-08 Final/Endless（先補齊完整行為證據鏈）
2. GS-07 Gate/Recovery/Demotion（30 seeds 行為批次）
3. GS-05 + GS-06（商店經濟 + Build 五層交互）
4. GS-09 + GS-10（Meta + 詞庫演進）
5. 回填矩陣與最終報告
