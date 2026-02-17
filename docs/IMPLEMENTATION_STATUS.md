# 實作進度看板

> 使用方式：每次任務狀態改變就更新本檔。
> 狀態欄位限定：`Todo` / `In Progress` / `Done` / `Blocked`

---

## 一、總體進度

| 里程碑 | 狀態 | 完成度 |
|---|---|---:|
| M0 專案骨架 | Done | 100% |
| M1 核心迴圈 | Done | 100% |
| M2 學習迴圈 | Done | 100% |
| M3 局外迴圈 | Done | 100% |
| M4 內容平衡 | Done | 100% |
| Alpha Gate | In Progress | 40% |

---

## 二、任務追蹤

| ID | 任務 | 里程碑 | Owner | 狀態 | 備註 |
|---|---|---|---|---|---|
| M0-01 | Unity 專案骨架與目錄 | M0 | Codex | Done | `Packages/`、`ProjectSettings/`、`Assets/` 已建立 |
| M0-02 | 五大 V2 Manager stubs | M0 | Codex | Done | `Run/Scoring/Learning/Shop/Meta` stubs 已可編譯 |
| M0-03 | Config 驗證串接 | M0 | Codex | Done | `configs/*.json` 範例 + `scripts/validate_configs.*` 可執行 |
| M0-04 | 最小測試入口 | M0 | Codex | Done | EditMode asmdef + manager stub tests 已建立 |
| M1-01 | 牌型判定引擎 | M1 | Codex | Done | 高到低優先級判定 + EditMode 測試完成 |
| M1-02 | 分數公式與拆解 | M1 | Codex | Done | SoT 成長值、答錯懲罰、ScoreBreakdown 細節已落地 |
| M1-03 | 盲注流程 | M1 | Codex | Done | Run 狀態機、盲注結算、商店後推進流程已實作與測試 |
| M1-04 | 商店流程 | M1 | Codex | Done | Offer 生成、價格帶、購買扣款與失敗處理已實作與測試 |
| M2-01 | Lv0-4 行為模型 | M2 | Codex | Done | 題型/限時/籌碼係數與 Boss Lv4->Lv3 規則已實作與測試 |
| M2-02 | 答錯三選一 | M2 | Codex | Done | 接受/重答/賭一把決策 API 與成本規則已實作與測試 |
| M2-03 | 退化規則 | M2 | Claude | Done | DecayManagerV2 + IDecayService + 9 test cases（1/3/7 天退化間隔） |
| M2-04 | Boss 學習規則 | M2 | Claude | Done | Boss +1 階、連對 x2、全對升級已實作與測試 |
| M3-01 | XP/LP 結算 | M3 | Claude | Done | XP=Ante*20+ClearBonus50, LP=Ante*2+ClearBonus5 |
| M3-02 | 契約系統 | M3 | Claude | Done | 11 種契約池、seed 決定論生成 3 張、結算 API |
| M3-03 | LP 上限守門 | M3 | Claude | Done | SettleContractWithCap: LP 契約獎勵上限 45/(100-45) |
| M3-04 | 課程樹 MVP 串接 | M3 | Codex | Done | TryUnlockNode 已支援前置/互斥/LP 守門與測試 |
| M4-01 | 詞庫內容填充 | M4 | Codex | Done | T1/T2 詞庫 100 筆（50/50）與覆蓋檢查已完成 |
| M4-02 | 商店權重平衡 | M4 | Codex | Done | Ante 分段權重 + Boss 課程卡 2 選 1 + 價格帶測試完成 |
| M4-03 | 盲注曲線平衡 | M4 | Codex | Done | RunDifficultyProfile 三檔曲線（Relaxed/Standard/Challenging）與測試完成 |
| M4-04 | 首輪平衡報告 | M4 | Codex | Done | `docs/21-m4-first-balance-report.md` 已完成 |
| A-01 | 全流程回歸 | Alpha | Codex | Done | 可授權 Unity 環境已執行 EditMode 回歸；compile fix（IsExternalInit）已修正並驗證通過 |
| A-UI-01 | 可操作卡牌 UI 原型 | Alpha | Codex | Done | 已完成真實卡牌 UI、拖曳上桌、商店卡片格、答題區（英文題幹/中文選項）、可收合調參面板 |
| A-BAL-01 | EffectiveVocab 關卡門檻 | Alpha | Codex | In Progress | GateProgression API + 原型流程卡關已接入；10模型第二輪實測 M0~M8 對齊、M9 可通關 |
| A-BAL-02 | Recovery Gate 與退回保護 | Alpha | Codex | In Progress | 回補關 + 7 天退關保護已接入原型流程 |
| A-BAL-03 | Boss 主動回憶題守門 | Alpha | Codex | In Progress | Boss 主動回憶雙門檻已接入盲注結算 |
| A-DATA-01 | 學習 telemetry 與告警 | Alpha | Codex | In Progress | `GATE_TOO_EASY/HARD`、`RECOVERY_FAILING` 告警已落地 |
| A-END-01 | Final 雙門檻（95/100） | Alpha | Codex | In Progress | Main Clear 95%、True Clear 100%+7 天已接入原型流程 |
| A-02 | 存檔/migration 壓測 | Alpha | TBD | Todo | |
| A-03 | 效能與穩定性驗收 | Alpha | TBD | Todo | |
| A-04 | 發版決策 | Alpha | TBD | Todo | |
