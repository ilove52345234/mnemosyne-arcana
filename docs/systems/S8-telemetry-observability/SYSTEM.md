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
- 現況：規則型測試通過。
- 調整建議：
1. 補誤報/漏報行為測試（以完整遊戲流程觸發）。
2. 將告警與調參動作建立固定回饋回路。

## 5. 更新紀錄
- 2026-02-18：改為系統自洽文件，不再使用跨文件引用描述。
