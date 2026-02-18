# 24 - 單字量成長曲線與關卡卡點追蹤方案

> 目的：將「0~10000 單字量」的學習曲線、卡關機制、遺忘退回、通關與無盡模式串成可落地且可追蹤的規格。

## 1. 問題定義

目前 `Standard` 盲注曲線已落地，但以固定單一玩家假設做驗證會失真，顯示存在以下風險：

1. 關卡壓力與詞彙熟練度尚未緊密綁定。
2. 玩家可能靠偶爾猜對而非穩定掌握通關。
3. 遺忘曲線對主流程卡點影響不足，缺少有效退回機制。
4. 缺少「學習成效型 telemetry」難以持續調參。

## 2. 10 段單字量模型（0~10000）

| 模型 | 目標詞彙量 | 預期可穩過 | 預期卡點 |
|---|---:|---|---|
| M0 | 0 | 教學關 | G1 |
| M1 | 2000 | G1 | G2 |
| M2 | 3000 | G2 | G3 |
| M3 | 4000 | G3 | G4 |
| M4 | 5000 | G4 | G5 |
| M5 | 6000 | G5 | G6 |
| M6 | 7000 | G6 | G7 |
| M7 | 8000 | G7 | G8 |
| M8 | 9000 | G8 | G9 |
| M9 | 10000 | Final（可通關） | 無（解鎖全域無盡） |

## 3. 有效詞彙量門檻（防猜通關）

關卡門檻使用「有效詞彙量」而非名目詞數：

`EffectiveVocab = LearnedCount * RetentionRate * RetrievalRate`

說明：
- `LearnedCount`：達到最低熟練等級的詞數（例如 Lv2+）。
- `RetentionRate`：到期複習詞在觀察窗（7/14 天）內保留比例。
- `RetrievalRate`：主動回憶題（拼字/聽寫/造句）正確率。

通關需同時滿足：
1. 最近 3 次挑戰，至少 2 次成功。
2. 該關核心詞覆蓋率 >= 85%（可配置）。
3. 主動回憶題占比 >= 40%，且正確率 >= 80%（可配置）。

## 4. 遺忘與退回機制

1. 每日退化檢查：沿用 1/3/7 天退化規則（Lv1/Lv2/Lv3），Lv4 退為 Lv3。
2. 保級檢查：若關卡核心詞覆蓋率跌破門檻，先進入 Recovery Gate（回補關）。
3. 退回規則：連續 2 個保級週期未恢復，退回前一關。
4. 保護機制：7 天內最多觸發 1 次實際退關，避免挫折爆量。

## 5. 終局與無盡模式

1. Final 採雙門檻：
- Main Clear：掌握率 >= 95%（可結業，解鎖全域無盡）
- True Clear：掌握率 = 100% 且連續穩定 7 天（真結局）
2. 每關首次通關即解鎖該關 Endless（該關詞池 + 詞條變體）。
3. Main Clear 後解鎖 Global Endless（全詞池混合，目標為分數上限）。
4. Endless 成績僅作排名與學習驅動，但掌握率與穩定天數會持續影響 True Clear 進度。

## 6. 追蹤指標（每週檢視）

1. `PassRateByGate`：各關卡通過率。
2. `RecoverySuccessRate`：進入回補關後 3 次內回復主線比例。
3. `ActiveRecallAccuracy`：主動回憶題正確率（關卡分層）。
4. `DecayRegressionRate`：因遺忘觸發退回的比例。
5. `GateStallDays`：同一卡點平均停留天數。

告警規則（初版）：
- 某關通過率 > 85%：壓力不足，需上調門檻或題型難度。
- 某關通過率 < 35%：挫折過高，需下調門檻或增強回補資源。
- `RecoverySuccessRate < 50%`：回補關設計失效，需重設內容密度。

## 7. 十模型驗證策略（取代固定 70% 玩家）

1. 驗證主軸：`M0~M9`（0, 2000, 3000 ... 10000）逐一跑流程，不再使用固定 70% 單一假設。
2. 每模型驗證輸出：
- 首次失敗關卡（Ante + Blind）
- 是否可達 RunComplete
- 是否觸發回補關/退回/告警
3. 比對基準：
- 預期卡點 `ExpectedChokeAnte`
- 實測卡點 `ObservedChokeAnte`
4. Gate：
- 若卡點偏差連續 2 次超過 1 關，必須進入平衡調參流程。

## 8. 實作追蹤任務（對應看板）

1. `A-BAL-01`：落地 EffectiveVocab 計算與 gate 判定 API。
2. `A-BAL-02`：落地 Recovery Gate + 退回保護機制。
3. `A-BAL-03`：落地 Boss 主動回憶題占比守門。
4. `A-DATA-01`：落地學習成效 telemetry 與告警閾值。

## 9. 執行進度（2026-02-17）

1. `A-BAL-01`：In Progress  
已落地 `GateProgressionManagerV2` 與 `GateProgressionTests`（10 模型 + EffectiveVocab + gate pass 判定），並已接入原型流程卡關判定。
2. `A-BAL-02`：In Progress  
已落地 `EvaluateRecoveryGate` 規則 API（回補關、連續 2 週期才可退回、7 天退關保護窗），並已接入原型失敗流程。
3. `A-BAL-03`：In Progress  
已落地 `EvaluateBossRecallGate` 守門 API（主動回憶題占比/正確率雙門檻），並已接入原型 Boss 結算守門。
4. `A-DATA-01`：In Progress  
已落地 `LearningTelemetryManagerV2.EvaluateAlerts`（`GATE_TOO_EASY/GATE_TOO_HARD/RECOVERY_FAILING`），原型流程已可輸出告警 log。
5. `A-END-01`：In Progress  
已落地 `EvaluateFinalMasteryGate`（95% Main Clear / 100%+7 天 True Clear），並接入原型 Final 結算。
6. `A-VAL-10M`：In Progress  
原型流程已改為 `10模型驗證`（取代固定 70% 假設），可輸出 M0~M9 實測卡關點。
