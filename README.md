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
