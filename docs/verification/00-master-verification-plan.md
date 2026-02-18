# 00 - Master Verification Plan

> 文件定位：長期基線規範（Baseline）
> 本輪執行計畫：`docs/verification/05-game-system-reverification-plan-2026-02-18.md`

## 1. 目標
- 建立單一驗測標準，覆蓋所有設計文件中的主系統。
- 每個主系統至少使用 3 種模型驗證，避免單一玩家假設造成偏差。
- 在進入 `A-02`（存檔/migration）前，必須完成全覆蓋驗測並有報告證據。

## 2. 強制規範
- 規範 V-01：每個主系統至少 `3` 模型（低/中/高），建議 `4` 模型（含極端）。
- 規範 V-02：每個模型至少 `3` seeds；平衡核心系統至少 `30` 輪批次。
- 規範 V-03：每次驗測必須產出可追溯證據（MCP log、測試摘要、文件回填）。
- 規範 V-04：設計文件覆蓋矩陣不得有 `Not Started` 後才能進 `A-02`。

## 3. 主系統清單
- S1 Run/Blind 狀態機與關卡推進
- S2 Scoring/HandType 得分與牌型
- S3 Learning（答題、答錯三選一、Boss 學習）
- S4 Gate/Recovery/Demotion（有效詞彙門檻、回補關、退回）
- S5 Shop/Economy（商店池、價格帶、購買流程）
- S6 Meta/Contract/Curriculum（XP/LP、契約、課程樹）
- S7 Final Gate/Endless（95% 主線、100%+7 天真通關、無盡模式）
- S8 Telemetry/Observability（告警與指標）
- S9 NFR（效能、穩定性、正確性門檻）

## 4. 驗測階段
1. Baseline：現況回歸（EditMode + 既有 PlayMode 流程）
2. Model Sweep：各系統 3+ 模型覆蓋
3. Stress：長週期遺忘/退回與高輪次平衡壓測
4. Sign-off：彙整最終報告與出關判定

## 5. 出關條件（Go/No-Go）
- `docs/verification/02-design-doc-coverage-matrix.md` 全部系統為 `Covered`
- `docs/verification/03-final-verification-report-template.md` 產出實際報告
- 主要風險無 `Critical` 未解

## 6. 執行產物
- 模型庫：`docs/verification/01-system-model-library.md`
- 覆蓋矩陣：`docs/verification/02-design-doc-coverage-matrix.md`
- 最終報告：`docs/verification/03-final-verification-report-template.md`

## 7. MCP 連線作業規範
- MCP endpoint 統一使用：`http://127.0.0.1:8080/mcp`。
- 任一 MCP 呼叫超過 `15 秒` 即判定失敗，進入恢復流程，不做無限等待。
- 失敗恢復順序：
1. 停止 Play Mode
2. 確認 Unity plugin 與 HTTP server 已啟動
3. 重啟 Codex session 重新建立 MCP transport
