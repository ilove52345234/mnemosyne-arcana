# S10 參考截圖分析與頁面重構規劃

## 範圍
- 來源：`docs/ui_reference/1.jpg` ~ `docs/ui_reference/9.jpg`
- 目標：以 Balatro 手機橫向遊玩為基準，重構 S10 的分頁與元件層級。

## 觀察摘要
- 核心結構固定：
  - 左側「Run HUD」長駐（分數、手數、棄牌、資源、按鈕）
  - 中央「主互動舞台」隨頁面切換
  - 右側保留牌堆/消耗品信息
- 操作節奏：
  - 盲注選擇 -> 出牌回合 -> 結算/商店 -> 下一回合
  - Modal 與 Tooltip 用於暫時資訊，不破壞底層主舞台

## 圖片對應到頁面
- `1.jpg`：商店主頁（Shop）
- `2.jpg`：小盲注結算頁（Settlement）
- `3.jpg`、`7.jpg`：盲注選擇頁（Blind Select）
- `4.jpg`、`8.jpg`：出牌主頁（Run Table）
- `5.jpg`：戰鬥中 Tooltip/說明泡泡（Popup/Tooltip）
- `6.jpg`：暫停選單（Pause Menu）
- `9.jpg`：比賽資訊/手牌資料 Modal（Run Info Modal）

## 重構後頁面清單
1. `P00-shared-shell`：共用框架（左 HUD、背景、右側堆疊資訊）
2. `P01-blind-select`：盲注選擇
3. `P02-run-table`：出牌主舞台
4. `P03-settlement`：回合結算
5. `P04-shop`：商店
6. `P05-pause-menu`：暫停與系統導覽
7. `P06-run-info-modal`：比賽資訊/牌型資訊
8. `P07-tooltip-and-popups`：戰鬥浮層（說明、效果、提示）

## 設計原則（本專案化）
- 主流程按鈕永遠不超過 3 個主 CTA。
- Modal 可讀完即離開，不承載主流程長操作。
- Tooltip 只做「上下文補充」，不做決策入口。
- 任何新頁面都先歸屬到既有 P00~P07，不新增雜項頁。
