# S7 - Final Gate/Endless

## 1. 設計規劃
- 目標：區分「可完成」與「真精通」。
- 核心原則：Main Clear 與 True Clear 分離，避免短期運氣誤判精通。

## 2. 規格文件
- Main Clear：mastery >= 95%。
- True Clear：mastery = 100% 且 stableDays >= 7。
- Endless：通關後開啟長局模式，觀察穩定性與非法狀態轉移。
- 驗測模型：Low / Mid / High + Edge（長局多 seed）。

## 3. 實作紀錄
- 已落地 final gate 決策 API。
- 已加入 S7 驗測組與 30 seeds 長局模擬。

## 4. 驗測報告與調整建議
- 最新結果（2026-02-18，round-3）：GS-08 驗測通過，且高模型通關帶維持在目標區間；`Done`（已決策同意）。
- round-3 證據：
  - `c74eb22c0ac841778133f41195e6415f`（EditMode：176/176）
  - `S7_M9_ThirtyRuns_ClearRateMonitoring`：`16/30 = 53.3%`（目標區間 `30%~60%`）
- round-2 證據：
  - `2fad6495060d4df69e36460981cb5794`（EditMode：175/175）
  - S7 對應測項：
    - `S7_M1_LowProfile_CannotPassMainClear`
    - `S7_M2_MidProfile_PassesMainClearOnly`
    - `S7_M3_HighProfile_PassesTrueClearAfterSevenStableDays`
    - `S7_M4_EdgeProfile_EndlessLongRun_IsStableAcrossThirtySeeds`
- 本輪設計問題：
1. 本輪未觀察到設計問題。
- 調整建議：
1. 先不改 95/100+7d 核心門檻。
2. M9 通關率目前 `53.3%`，仍落在目標帶內，先不調參。
3. 若連續兩輪高於 `60%`，再下修高模型成長增益；若低於 `30%`，再上修。

## 5. 更新紀錄
- 2026-02-18：完成高模型通關帶 round-3 監控，新增 `S7_M9_ThirtyRuns_ClearRateMonitoring`；結果 `16/30（53.3%）`。
- 2026-02-18：完成 GS-08 驗測 round-2（4/4 pass），新增 job `2fad6495060d4df69e36460981cb5794` 證據。
- 2026-02-18：完成 GS-08 驗測一輪（4/4 pass）。
- 2026-02-18：改為系統自洽文件，不再使用跨文件引用描述。
