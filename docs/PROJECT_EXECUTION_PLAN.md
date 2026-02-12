# 專案執行總排程（PM）

> 專案：Mnemosyne Arcana
> 更新方式：每次完成任務後，必須同步更新本檔、`docs/IMPLEMENTATION_STATUS.md`、`docs/SESSION_NOTES.md`

---

## 一、里程碑總覽（建議 8 週）

| 週期 | 里程碑 | 目標 | 主要負責 |
|---|---|---|---|
| W1 | M0 啟動 | 建立可開發骨架 | 技術主導 + 工具 |
| W2-W3 | M1 核心迴圈 | 出牌->判定->得分->盲注->商店 | Gameplay |
| W4-W5 | M2 學習迴圈 | Lv0-4、答錯三選一、退化 | Learning/System |
| W6 | M3 局外迴圈 | XP/LP、契約、課程樹 MVP | Meta/System |
| W7 | M4 內容平衡 | 詞庫/商店/曲線首輪調平 | 設計 + 數據 |
| W8 | Alpha Gate | 回歸、效能、存檔/migration 驗收 | QA + 全員 |

### 目前進度（2026-02-12）

- M0-01 ~ M0-04 已完成，M0 可結案。
- M1-01 已完成（牌型判定引擎 + 測試案例）。
- M1-02 已完成（SoT 成長值 + 答錯懲罰 + 公式拆解）。
- 目前主軸：M1-03（盲注流程）接續 M1-04（商店流程）。

---

## 二、工作流與更新規則

1. 每個任務開始前：
- 在 `docs/IMPLEMENTATION_STATUS.md` 建立任務列（狀態：`In Progress`）

2. 每次工作結束後：
- 在 `docs/SESSION_NOTES.md` 新增一筆「交接記錄」（日期、內容、結果、下一步）
- 更新 `docs/IMPLEMENTATION_STATUS.md` 對應任務狀態（`Done/Blocked`）
- 若規格有改動，同步更新對應文件（01~18）

3. 任何破壞性改動：
- 需先更新 `docs/11-risk-register-and-decision-log.md`

---

## 三、M0-M4 派工清單（可直接分配）

### M0：專案骨架（W1）

- 任務 M0-01：建立 Unity 專案骨架與目錄
- 任務 M0-02：建立 RunManagerV2 / ScoringManagerV2 / LearningManagerV2 / ShopManagerV2 / MetaManagerV2 stubs
- 任務 M0-03：接上 config 驗證腳本（`scripts/validate_configs.*`）
- 任務 M0-04：建立最小測試入口（EditMode）

### M1：核心迴圈（W2-W3）

- 任務 M1-01：牌型判定引擎
- 任務 M1-02：分數公式與結果拆解
- 任務 M1-03：盲注通關/失敗流程
- 任務 M1-04：商店進出與購買流程

### M2：學習迴圈（W4-W5）

- 任務 M2-01：Lv0-4 行為模型
- 任務 M2-02：答錯三選一（接受/重答/賭一把）
- 任務 M2-03：退化規則（1/3/7 天）
- 任務 M2-04：Boss 關題型升級與全對獎勵

### M3：局外迴圈（W6）

- 任務 M3-01：XP/LP 結算
- 任務 M3-02：契約 3 選 1 + 刷新 + 跳過
- 任務 M3-03：契約 LP 45% 上限守門
- 任務 M3-04：課程樹 MVP 節點串接

### M4：內容與平衡（W7）

- 任務 M4-01：詞庫 T1/T2 可玩內容
- 任務 M4-02：商店池權重與價格帶調整
- 任務 M4-03：盲注曲線與 Ante 體感調整
- 任務 M4-04：首輪回歸平衡報告

### Alpha Gate（W8）

- 任務 A-01：Ante 1-8 全流程回歸
- 任務 A-02：存檔/migration 壓測
- 任務 A-03：效能與穩定性驗收（參照 `docs/09-nfr-and-quality-gates.md`）
- 任務 A-04：發版決策會議

---

## 四、完成定義（DoD）

每項任務都必須滿足：
1. 對應測試通過
2. 文件已更新
3. 可被下一位接手者理解並延續
