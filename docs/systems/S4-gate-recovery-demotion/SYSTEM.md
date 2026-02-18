# S4 - Gate/Recovery/Demotion

## 1. 設計規劃
- 目標：卡關合理、回補可行、退回可控。
- 核心原則：不能靠短期運氣越級，也不能無限打轉。

## 2. 規格文件
- 有效詞彙量：EffectiveVocab = LearnedCount * RetentionRate * RetrievalRate。
- 通關守門條件：覆蓋率與主動回憶表現需同時達標。
- Recovery Gate：先回補，再判定是否回主線。
- Demotion：連續未恢復才退回前關，且有保護窗避免頻繁退關。
- 長週期退化：以 7/14/30 天作為關鍵觀測點，要求趨勢單調。

## 3. 實作紀錄
- 已接入 gate progression 決策與 recovery 流程。
- 已完成長週期分布測試與排序驗證。

## 4. 驗測報告與調整建議
- 驗測結論（2026-02-18，重啟 S4）：驗測完成，`Done` 待你決策（含高負載要求）。
- 三模型對應：
  - `M-Low`：`S4PriorityValidationTests.RecoveryGate_ThreeModelProfiles_MatchPassCriteria`（低覆蓋 + 失敗循環保護）。
  - `M-Mid`：同測項中段模型（需 recovery、避免過早 demotion）。
  - `M-High`：同測項高模型（達標直接 pass，不誤觸 recovery）。
- Edge/批次要求：
  - `S4LongCycleDistributionTests.S4_RecoveryGate_ThirtySeedDistribution_ShowsExpectedOrdering`（30 seeds 排序檢查）。
  - `S4LongCycleDistributionTests.S4_Decay_LongCycle_SevenFourteenThirtyDays_IsMonotonic`（7/14/30 天單調退化）。
- 失敗/邊界案例：
  - 低模型在連續失敗循環下仍受 7 天保護窗約束（不過度退關）。
  - 長週期檢查確保不發生逆向升級（單調性邊界）。
- 證據（MCP job）：
  - `9806eac7df504e12b3275f669096e5e9`（S4PriorityValidationTests：3/3）
  - `59f41d6790f24d46a1ec65e7c9249acf`（S4LongCycleDistributionTests：2/2）
- 重跑證據（MCP job，2026-02-18）：
  - `44f4ab6e9a76495b9613b0a5718ba8f6`（S4PriorityValidationTests：3/3）
  - `d17c868c876f4bb0ac96741253282a43`（S4LongCycleDistributionTests：2/2）
- 本輪設計問題：
1. 本輪未觀察到新的設計問題（三模型判定、30-seed 排序、7/14/30 單調退化均符合規格）。
- 調整建議（小幅）：
1. 若 M-Low 停滯天數偏長，可把 recovery 成功後增益 `+0.01 -> +0.015`。
2. 若高段進展過快，可把 demotion 後懲罰 `-0.03 -> -0.035`。
3. 先不動 7/14/30 退化門檻本體，優先調 recovery/demotion 幅度。

## 5. 更新紀錄
- 2026-02-18：完成 GS-07 驗測一輪（3/3 + 2/2 pass）。
- 2026-02-18：改為系統自洽文件，不再使用跨文件引用描述。
- 2026-02-18：完成 S4 done 驗測收斂（Low/Mid/High + 30 seeds + long-cycle monotonic）。
- 2026-02-18：依新規則重啟 S4 驗測，補充「本輪設計問題/調整建議/待你決策 Done」與重跑證據。
