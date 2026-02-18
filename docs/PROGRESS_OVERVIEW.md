# Progress Overview (Single Source)

> 專案唯一進度總表。所有系統完成度以本檔為準。

## 狀態定義（統一）

- `Todo`：尚未開始，沒有可驗證產物。
- `In Progress`：已開始，已有部分產物，但未達 Done 門檻。
- `Done`：已滿足本檔「Done 量化門檻」且證據完整。
- `Blocked`：因外部阻塞無法推進（需記錄阻塞原因與解除條件）。

## Done 量化門檻（硬指標）

### A. 欄位級門檻

- `Design = Done`：有明確目標、範圍、非目標與風險（至少 1 條）。
- `Spec = Done`：規則可執行，至少包含輸入/輸出、邊界條件、失敗處理。
- `Implementation = Done`：功能可重現；至少 1 條實作紀錄與 1 條變更紀錄。
- `Verification = Done`：
  1) 至少 `M-Low/M-Mid/M-High` 三模型完成。
  2) 核心平衡系統（S4/S7/S9）需加 `M-Edge` 或 `>=30 seeds` 批次。
  3) 測試通過率 `100%`（該系統對應測項）。
  4) 至少 1 個失敗/邊界案例被驗證。
  5) 具可追溯證據（job id 或等價執行紀錄）。

### B. 系統級門檻

- `System Status = Done` 必須同時滿足：
  1) `Design/Spec/Implementation/Verification` 四欄全部為 `Done`。
  2) `SYSTEM.md` 第 4 節含「結果 + 參數建議」。
  3) `SYSTEM.md` 第 5 節有最近一次更新日期。
  4) 本表 `Last Update` 為最近更新日期，`Next Action` 可留空或標記 `Monitor`。

### C. Blocked 管理門檻

- `Status = Blocked` 必須加註：
  1) 阻塞原因
  2) 解除條件
  3) 預計檢查時間（日期）

| System | Status | Design | Spec | Implementation | Verification | Last Update | Next Action |
|---|---|---|---|---|---|---|---|
| S1 Run/Blind | Done | Done | Done | Done | Done | 2026-02-18 | Monitor |
| S2 Scoring/HandType | Done | Done | Done | Done | Done | 2026-02-18 | Monitor |
| S3 Learning/Boss | Done | Done | Done | Done | Done | 2026-02-18 | Monitor |
| S4 Gate/Recovery/Demotion | Done | Done | Done | Done | Done | 2026-02-18 | Monitor |
| S5 Shop/Economy | In Progress | Done | Done | Done | In Progress | 2026-02-18 | 補重擲經濟壓測 |
| S6 Meta/Contract/Curriculum | In Progress | Done | Done | Done | In Progress | 2026-02-18 | 補全樹行為驗測 |
| S7 Final/Endless | In Progress | Done | Done | Done | Done (round-1) | 2026-02-18 | 監控高模型通關帶 |
| S8 Telemetry/Observability | In Progress | Done | Done | Done | In Progress | 2026-02-18 | 補誤報/漏報場景 |
| S9 NFR/Quality | In Progress | Done | Done | Done | In Progress | 2026-02-18 | 建 soak 趨勢報表 |

## 更新規範
1. 每完成一輪系統驗測，先更新對應 `docs/systems/Sx-*/SYSTEM.md` 的第 4/5 區塊。
2. 再更新本檔 `Verification`、`Last Update`、`Next Action`。
3. 狀態僅使用：Todo / In Progress / Done / Blocked。
4. 任何欄位標記 `Done` 前，必須逐條檢查本檔 Done 量化門檻。
