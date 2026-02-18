# Mnemosyne Arcana

一款把英語詞彙學習自然嵌入玩法的卡牌 Roguelike。

## 專案目標

建立一套可長期迭代的卡牌遊戲系統，核心原則：

- 驗算規則簡單（玩家一眼可懂）
- Build 深度足夠（每局都有策略差異）
- 學習自然發生（不做考試式中斷）

## 開發前必讀

開始任何開發前，**必須完整閱讀 `docs/` 底下所有文件**（含 `docs/adr/` 與 `docs/archive/` 歷史資料）。

建議閱讀順序：
1. `docs/06-onboarding-checklist.md`
2. `docs/00-project-vision.md`
3. `docs/01-game-design-core.md`
4. `docs/02-meta-progression.md`
5. `docs/03-technical-architecture.md`
6. `docs/04-data-contracts.md`
7. `docs/05-development-workflow.md`
8. `docs/07-roadmap-mvp.md`
9. `docs/09-nfr-and-quality-gates.md`
10. `docs/10-runtime-state-and-event-contracts.md`
11. `docs/11-risk-register-and-decision-log.md`
12. `docs/13-system-context.md`
13. `docs/14-core-usecase-sequences.md`
14. `docs/15-balance-source-of-truth.md`
15. `docs/16-config-governance.md`
16. `docs/17-test-matrix.md`
17. `docs/PROJECT_EXECUTION_PLAN.md`
18. `docs/IMPLEMENTATION_STATUS.md`
19. `docs/SESSION_NOTES.md`
20. `CLAUDE.md`
21. `docs/19-system-development-boundaries.md`

## Docs 導覽（路徑與用途）

### 目錄級用途

- `docs/`：專案主規格、流程、進度與驗測文件（唯一官方來源）。
- `docs/adr/`：架構決策紀錄（為何這樣選型/取捨）。
- `docs/plans/`：分任務實作計畫與設計稿（按日期追蹤）。
- `docs/verification/`：驗測主計畫、覆蓋矩陣、最終報告與操作規範。
- `docs/schemas/`：資料契約 JSON Schema。
- `docs/reference/`：外部參考資料（原始 PDF/文章）。
- `docs/archive/`：歷史版本與已退役方案（僅供追溯，不作現行規格）。

### 核心文件用途（常用）

- `docs/00-project-vision.md`：產品願景、成功標準與非目標。
- `docs/01-game-design-core.md`：局內核心玩法規則（Run、牌型、分數、Boss）。
- `docs/02-meta-progression.md`：局外成長系統（XP/LP、課程樹、契約）。
- `docs/03-technical-architecture.md`：技術架構與模組邊界。
- `docs/04-data-contracts.md`：資料模型、版本與 migration 規格。
- `docs/05-development-workflow.md`：分支、DoD、開發流程。
- `docs/06-onboarding-checklist.md`：新接手者首日清單與驗收題。
- `docs/07-roadmap-mvp.md`：M0~M4 + Alpha 的階段路線。
- `docs/08-design-traceability.md`：設計追溯關係與覆蓋率策略。
- `docs/09-nfr-and-quality-gates.md`：效能/穩定性/品質門檻。
- `docs/10-runtime-state-and-event-contracts.md`：執行期狀態與事件契約。
- `docs/11-risk-register-and-decision-log.md`：風險清單與重要決策。
- `docs/12-architecture-gap-closure.md`：架構缺口補齊追蹤。
- `docs/13-system-context.md`：系統邊界與外部依賴。
- `docs/14-core-usecase-sequences.md`：核心用例序列（文字版）。
- `docs/15-balance-source-of-truth.md`：數值單一真相表（Balance SoT）。
- `docs/16-config-governance.md`：設定檔治理與 PR Gate。
- `docs/17-test-matrix.md`：需求到測試案例 ID 的映射。
- `docs/18-api-and-domain-types.md`：API/DTO/Enum 契約。
- `docs/19-system-development-boundaries.md`：不可衝突的系統邊界條款。
- `docs/PROJECT_EXECUTION_PLAN.md`：PM 主排程與里程碑。
- `docs/IMPLEMENTATION_STATUS.md`：任務看板（Todo/In Progress/Done/Blocked）。
- `docs/SESSION_NOTES.md`：每次工作交接記錄（下一步依此銜接）。
- `docs/20-project-analysis-2026-02-14.md`：專案盤點與下一輪建議。
- `docs/21-m4-first-balance-report.md`：M4 首輪平衡結論。
- `docs/22-alpha-a01-regression-checklist.md`：A-01 回歸驗收清單。
- `docs/23-user-stories-and-use-cases.md`：Alpha 可玩流程故事/用例覆蓋。
- `docs/24-vocab-growth-curve-and-gating-plan.md`：詞彙成長曲線與卡關機制規格。
- `docs/25-gate-model-sweep-report-2026-02-17.md`：Gate 模型 sweep 與分佈報告。

### Verification 文件用途

- `docs/verification/00-master-verification-plan.md`：全系統驗測總規範與出關條件。
- `docs/verification/01-system-model-library.md`：S1~S9 模型定義（Low/Mid/High/Edge）。
- `docs/verification/02-design-doc-coverage-matrix.md`：設計文件覆蓋狀態（Not Started/In Progress/Covered）。
- `docs/verification/03-final-verification-report-template.md`：最終報告模板。
- `docs/verification/03-final-verification-report-2026-02-18.md`：本輪實際最終驗測報告。
- `docs/verification/04-mcp-connection-recovery-checklist.md`：MCP 連線失敗恢復 SOP。

## 目前階段

- 設計規格：可開發等級（持續補充）
- 實作狀態：M0~M4 全完成，進入 Alpha Gate（A-01 全流程回歸）

## 快速試玩（遊戲畫面原型）

1. 用 Unity Hub 開啟專案（版本 `2022.3.62f3`）。
2. 開任意 Scene（空場景也可），按 Editor 上方 `Play`。
3. 畫面會自動出現中文「遊戲畫面原型」：
- 左側：手牌區與出牌操作
- 中間：戰鬥/商店流程
- 右側：中文調參面板
- 右下：事件紀錄
 - 右側調參面板可「展開/收合」
 - 整體面板支援上下滾動（滑鼠滾輪）
 - 卡牌可拖曳到「牌桌區」上桌，也可點擊快速上桌
 - 上桌卡牌會顯示【上桌】狀態與高亮脈衝
 - 抽牌有進場動畫，出牌有飛出動畫
 - 商店為可點擊卡片格子（不是純文字清單）
4. 你可以直接操作：
- `開始答題並出牌`（逐題答完才會出牌結算）
  - 題型：英文題目 + 中文四選一
- `清空上桌`（重排本回合出牌）
- `結算盲注` / `前往下一關`
- `生成商店商品` / `購買第一項`
- `嘗試解鎖` / `生成契約` / `結算契約`

> 說明：這是給開發迭代用的可操作原型，採真實卡牌 UI 互動（可選牌/出牌/流程推進），並帶場景底板主題。非最終美術 UI。單字維持英文，其餘輔助面板皆為中文。

## 命名資訊

- 產品名稱：**Mnemosyne Arcana**
- Repo 名稱：`mnemosyne-arcana`
