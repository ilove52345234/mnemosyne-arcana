# UI 設計進入規範（強制）

## 目的
- 確保所有設計者在 S10 設計前，先完成同一份參考理解，避免風格漂移與重工。

## 強制流程（未完成不得開始設計）
1. 先閱讀 `docs/ui_reference` 全部截圖（1.jpg ~ 9.jpg）。
2. 閱讀 `docs/ui_reference/SCREEN_ANALYSIS_AND_PAGE_PLAN.md`。
3. 閱讀 `docs/systems/S10-ui-ux/SYSTEM.md` 與 `docs/systems/S10-ui-ux/ALIGNMENT_TRACKER.md`。
4. 在 PR/提交訊息附上「已完成前置閱讀」聲明，格式如下：
   - `S10-UI-ENTRY-CHECK: 1.jpg~9.jpg + SCREEN_ANALYSIS_AND_PAGE_PLAN + SYSTEM + ALIGNMENT_TRACKER 已閱讀`

## 禁止事項
- 未完成前置閱讀直接改版面。
- 只看單張截圖就下設計決策。
- 跳過頁面分工，將所有改動混在同一個大文件。

## 設計輸出要求
- 每個頁面必須在各自資料夾維護：
  - `SYSTEM.md`（頁面目標、互動、狀態、驗收）
  - `BASELINE_REFERENCE.md`（對應參考圖與差距）
- 橫向手機（landscape）為唯一主基準。
