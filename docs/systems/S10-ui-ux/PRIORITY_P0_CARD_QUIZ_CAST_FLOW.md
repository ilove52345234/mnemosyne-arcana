# S10-P0 優先落地：選牌答題後出卡（Card Quiz Cast Flow）

## 1. 目標
- 對齊 Balatro「按出牌後依序打牌動畫」節奏。
- 融合英語學習：先答題，再出卡結算。
- 卡牌改為雙面：正面保留學習屬性，背面揭露答案與美術（美術先留空）。

## 2. 卡面規格（雙面）
- 正面（選牌/答題前可見）：
  - 英語（word）
  - 詞性（pos）
  - 元素（element）
  - 等級（level）
  - 移除「上桌」字樣
- 背面（答題完成後翻牌可見）：
  - 正確答案（文字）
  - 回答結果（正確/錯誤）
  - 美術圖層佔位（`art_placeholder`）

## 3. 互動狀態機（單回合）
1. `HandSelect`
2. `CastIntentLocked`（點擊「出牌」後鎖定選牌）
3. `QuizFocusIn`（中央放大單卡）
4. `QuizQuestionActive`（顯示題目 UI）
5. `QuizAnswerFeedback`（單題結果回饋）
6. `QuizAdvanceNextCard`（進下一張）
7. `QuizCompleted`
8. `CastAnimationQueue`（依序出卡）
9. `CardFlipReveal`（翻轉揭露答案/背面）
10. `ResolveScore`
11. `RoundPostState`

## 4. 事件與行為
- `OnCastPressed`
  - 鎖定手牌/拖曳輸入
  - 建立當前出牌序列（保留玩家選牌順序）
- `OnQuestionRendered`
  - 題型由題庫策略決定（中文選項 / 拼字 / 發音按鈕）
- `OnAnswerSubmitted`
  - 記錄答題結果與耗時
  - 產生單題回饋（0.3~0.5 秒）
- `OnQuizAllDone`
  - 啟動出卡隊列動畫
- `OnCardCastFinished`
  - 觸發翻牌揭露與局部特效
- `OnCastQueueDone`
  - 進入計分

## 5. 題型策略（V1）
- `MCQ_ZH`：英文 -> 中文選項（主題型）
- `SPELLING`：拼字輸入（次題型）
- `AUDIO_RECOGNITION`：發音按鈕 + 識別（可先 UI 佔位，功能後補）

## 6. 動畫節奏（V1）
- 單張節奏：
  - 放大進場：180ms
  - 題目互動：玩家操作時間
  - 作答反饋：300ms
  - 出卡到桌面：220ms
  - 翻牌揭露：180ms
- 多張佇列：
  - 卡與卡間隔：80ms
  - 若連續答對可疊加節奏加速（後續）

## 7. 主要風險與對策
- 節奏過慢：
  - 對策：`Quick Mode`（縮短動畫 + 僅保留核心回饋）
- 狀態錯亂（輸入競態）：
  - 對策：答題期間全域鎖輸入，狀態機單向流轉
- 手機效能：
  - 對策：動畫品質分級（High/Low）、限制同屏特效數
- 題庫不完整：
  - 對策：先上 `MCQ_ZH`，其他題型 feature flag 控制

## 8. MVP 驗收（P0）
- 選好牌後點 `出牌` 進入「逐張答題」流程。
- 所有題目完成前，不觸發實際出卡。
- 題目完成後，卡牌依序打到桌面並翻牌。
- 翻牌後可見答案欄位與美術佔位。
- 全流程可在橫向手機比例穩定運行，無卡死。

## 9. 實作切分
- M1：狀態機骨架 + 輸入鎖 + 逐張題目控制器
- M2：題型 `MCQ_ZH` + 答題結果記錄
- M3：出卡隊列動畫 + 翻牌揭露（含答案/美術佔位）
- M4：計分接線 + 回合結束轉場
- M5：效能優化 + 快速模式
