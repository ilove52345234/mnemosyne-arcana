# S8 - Telemetry/Observability

## 1. 設計規劃
- 目標：讓調參以真實行為指標驅動。
- 核心原則：告警要可解釋，且與流程狀態一致。

## 2. 規格文件
- 最低事件：run_start/end、blind_start/result、hand_scored、contract_selected/settled、save_migrated/failed。
- 告警類型：GATE_TOO_HARD、GATE_TOO_EASY、RECOVERY_FAILING。
- 事件紀錄需可追溯 run 維度與版本維度。

## 3. 實作紀錄
- 已落地告警評估邏輯。
- 已完成 S8 三模型驗測用例。

## 4. 驗測報告與調整建議
- 驗測結論（2026-02-18，round-2）：告警規則與誤報/漏報場景驗測通過；`Done`（已決策同意）。
- 證據：
  - Unity MCP EditMode：`6dcd9412cb4241dc86131720501989a4`（`178/178 passed`）
- 三模型對應：
  - `M-Low`：`S8_M1_LowProfile_TriggersGateTooHard`
  - `M-Mid`：`S8_M2_MidProfile_RemainsWithinTarget_NoAlert`
  - `M-High`：`S8_M3_HighProfile_TriggersGateTooEasy`
- 失敗/邊界案例：
  - `S8_FP_HighPassButLowRecall_DoesNotTriggerTooEasy`（誤報防護）
  - `S8_FN_BorderlinePassWithLongStall_TriggersTooHard`（漏報補抓）
- 本輪設計問題：
1. 本輪未觀察到新增設計問題。
- 調整建議：
1. 維持現行門檻（`PassRate > 85%`、`PassRate < 35%`、`Recovery < 50%`），並保留「低主動回憶不判 TooEasy」與「長卡關補判 TooHard」的防誤報/漏報守門。
2. 下一輪可把 alert 與調參動作做固定映射（如 `GATE_TOO_HARD -> 降低目標分/提升學習增益`）以縮短回饋迴圈。

## 5. 更新紀錄
- 2026-02-18：完成 round-2 誤報/漏報驗測，新增 `S8_FP` / `S8_FN` 測項並通過（job `6dcd9412cb4241dc86131720501989a4`）。
- 2026-02-18：改為系統自洽文件，不再使用跨文件引用描述。
