# 08 - 設計追溯與覆蓋率

> 目的：確認舊專案關鍵討論已完整轉入新專案主規格。
> 範圍：
> - `docs/plans/2026-02-12-v2-core-design.md`
> - `docs/plans/2026-02-12-v2-meta-system-design.md`
> - `docs/plans/2026-02-12-redesign-handoff.md`
> - `docs/reference/zhihu-650859964/01.pdf`
> - `docs/reference/zhihu-650859964/02.pdf`
> - `docs/reference/zhihu-650859964/03.pdf`

---

## 1. 主規格文件（新專案）

- `docs/00-project-vision.md`
- `docs/01-game-design-core.md`
- `docs/02-meta-progression.md`
- `docs/03-technical-architecture.md`
- `docs/04-data-contracts.md`
- `docs/05-development-workflow.md`
- `docs/07-roadmap-mvp.md`

歷史參考保留於：
- `docs/archive/legacy-vocab-v2/`
- `docs/reference/zhihu-650859964/`

---

## 2. 覆蓋對映表

| 舊來源主題 | 新文件對應 | 覆蓋狀態 |
|---|---|---|
| v2 核心哲學（極簡驗證 + 複雜 Build） | `docs/00-project-vision.md` | 完整 |
| 10 種牌型與分數公式 | `docs/01-game-design-core.md` | 完整 |
| 學習等級、答錯不斷牌型、三選一懲罰 | `docs/01-game-design-core.md` | 完整 |
| Build 五層（語感/教材/詞綴/課程/頓悟） | `docs/01-game-design-core.md` | 完整 |
| 經濟系統與商店價格帶 | `docs/01-game-design-core.md` | 完整 |
| 盲注曲線與 Boss 規則 | `docs/01-game-design-core.md` | 完整 |
| 局外 Hybrid（學院+契約） | `docs/02-meta-progression.md` | 完整 |
| 4x12 課程樹完整節點 | `docs/02-meta-progression.md` | 完整 |
| 契約池權重/難度/獎勵邊界 | `docs/02-meta-progression.md` | 完整 |
| 詞庫層級門檻 | `docs/02-meta-progression.md` | 完整 |
| 詞庫抽樣演算法與滴入規則 | `docs/02-meta-progression.md` | 完整 |
| 模組邊界、狀態機、資料流 | `docs/03-technical-architecture.md` | 完整 |
| Schema、JSON 範例、Enum、驗證規則 | `docs/04-data-contracts.md` | 完整 |
| v1 -> v2 遷移策略 | `docs/04-data-contracts.md` | 完整 |
| 重設計背景與保留/捨棄判準 | `docs/00-project-vision.md`, `docs/04-data-contracts.md` | 完整 |
| 3 篇參考文結論（超級概念/節奏/隨機/經濟） | `docs/00-project-vision.md`, `docs/01-game-design-core.md`, `docs/02-meta-progression.md` | 完整 |

---

## 3. 差異處理策略

原始文件與新規格若有衝突，採以下優先序：

1. `docs/01-game-design-core.md`
2. `docs/02-meta-progression.md`
3. `docs/03-technical-architecture.md`
4. `docs/04-data-contracts.md`
5. `docs/archive/legacy-vocab-v2/*`（僅歷史參考）

---

## 4. 覆蓋率結論

- 核心玩法：100% 覆蓋
- 局外 Meta：100% 覆蓋
- 技術架構：100% 覆蓋
- 資料契約與遷移：100% 覆蓋
- 重設計背景與決策脈絡：100% 覆蓋

整體結論：
- 目前新專案主規格已可獨立開發。
- 歷史文件保留在 archive 與 reference，用於追溯，不作當前實作依據。
