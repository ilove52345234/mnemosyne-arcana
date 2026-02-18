# S10 UI 對齊追蹤（Balatro Mobile）

## 目的
- 作為 S10 手機版 UI 對齊的單一記錄檔。
- 每一輪循環結束（`Play -> 截圖 -> 對比 -> 調整`）都更新本檔，並推送遠端。

## 對齊量化標準（0-100）
- `Layout`（30）：區塊層次、主次視覺、留白節奏。
- `Readability`（25）：字級、資訊密度、手機可讀性。
- `Interaction`（25）：流程分頁、操作反饋、拖曳可理解性。
- `Polish`（20）：卡牌質感、陰影/邊框、色彩一致性。

`總分 = Layout + Readability + Interaction + Polish`

## 每輪固定更新欄位
- 輪次編號
- 截圖檔案路徑
- 當前分數（四項分解 + 總分）
- 本輪修改
- 未解決問題（自動延到下一輪）
- 下一輪優先事項

## 迭代紀錄

### Round R10（2026-02-19）
- Screenshots:
  - `Assets/Screenshots/S10-auto-loop-r10-m1-1.png`
- Score:
  - Layout: 24/30
  - Readability: 20/25
  - Interaction: 21/25
  - Polish: 12/20
  - Total: 77/100
- 本輪修改:
  - 實作 M1 狀態機骨架：`CardQuizCastPhase`（選牌 -> 答題 -> 出卡 -> 翻牌 -> 計分）。
  - 加入全域輸入鎖，避免答題/出卡期間誤觸（點牌、拖曳、清空、重抽、盲注結算）。
  - 既有 `StartQuizAndPlay` 流程改接新狀態階段，並在答題完成後才進入出卡動畫隊列。
  - 移除卡面「上桌」字樣，保留英語、詞性、元素、等級。
- 未解決問題:
  - 中央題目 UI 仍為舊版清單樣式，尚未改成放大單卡答題舞台。
  - 翻牌揭露目前為狀態流程與時序佔位，尚未完成雙面卡視覺。
- 下一輪優先:
  - M2：中央單卡放大答題舞台 + 題型切換容器（中文選項/拼字/發音按鈕）。
  - M3：雙面卡翻轉與答案揭露視覺層。

### Round R9（2026-02-19）
- Screenshots:
  - 無（本輪為 P0 規格落地）
- Score:
  - Layout: 24/30
  - Readability: 20/25
  - Interaction: 19/25
  - Polish: 12/20
  - Total: 75/100
- 本輪修改:
  - 新增 `S10-P0` 優先落地規格：`PRIORITY_P0_CARD_QUIZ_CAST_FLOW.md`。
  - 完成「選牌 -> 逐張答題 -> 依序出卡 -> 翻牌揭露」狀態機與事件定義。
  - 將 P0 流程掛接到 `P02-run-table` 與 S10 主系統文件。
- 未解決問題:
  - 尚未實作到程式層狀態機與動畫佇列。
- 下一輪優先:
  - 先做 M1：狀態機骨架 + 輸入鎖 + 逐張題目控制器。

### Round R8（2026-02-19）
- Screenshots:
  - `Assets/Screenshots/S10-auto-loop-r8-1.png`
- Score:
  - Layout: 24/30
  - Readability: 20/25
  - Interaction: 17/25
  - Polish: 12/20
  - Total: 73/100
- 本輪修改:
  - UI 縮放基準改為手機橫向：`CanvasScaler.referenceResolution = 2400x1080`，`matchWidthOrHeight = 1.0`。
  - 響應式新增 `landscape` 分支，側欄寬度與高度策略改為橫向優先。
  - 直式大高度下限（700）改為橫向最小高度（360）避免橫向壓縮時失真。
- 未解決問題:
  - 目前測得截圖仍為直式 Game 視窗，因此尚未完整驗證橫向排版成效。
- 下一輪優先:
  - 以橫向比例（20:9 或 19.5:9 landscape）重新截圖驗證。
  - 依橫向結果再微調側欄寬度與主區塊節奏。

### Round R7（2026-02-19）
- Screenshots:
  - `Assets/Screenshots/S10-auto-loop-r7-1.png`
  - `Assets/Screenshots/S10-auto-loop-r7-2.png`
  - `Assets/Screenshots/S10-auto-loop-r7-3.png`
- Score:
  - Layout: 23/30
  - Readability: 20/25
  - Interaction: 17/25
  - Polish: 12/20
  - Total: 72/100
- 本輪修改:
  - 手機模式操作按鈕字級提升（主按鈕/次按鈕）。
  - 手牌卡片最小寬度與字級提升。
  - 新增 `UpdateButtonTypography()`，確保響應式切換後字級會重新套用。
- 未解決問題:
  - 卡牌材質層次與高光仍不足，與 Balatro 手機版仍有質感落差。
  - 拖曳目標區的可視反饋仍偏弱。
- 下一輪優先:
  - 強化卡牌外框/陰影/高光。
  - 加入拖曳中高亮、放置成功反饋。

### Round R6（2026-02-19）
- Screenshots:
  - `Assets/Screenshots/S10-auto-loop-r6-1.png`
  - `Assets/Screenshots/S10-auto-loop-r6-2.png`
  - `Assets/Screenshots/S10-auto-loop-r6-4.png`
  - `Assets/Screenshots/S10-auto-loop-r6-5.png`
  - `Assets/Screenshots/S10-auto-loop-r6-6.png`
- Score:
  - Layout: 22/30
  - Readability: 18/25
  - Interaction: 17/25
  - Polish: 11/20
  - Total: 68/100
- 本輪修改:
  - 補上 `CanvasScaler` 參考解析度與 match 策略（手機比例縮放穩定）。
  - 手機模式狀態欄文案精簡。
  - 側欄資訊卡改短標題（分數/節奏/資源）。
  - 卡牌手機文案精簡（單字 + Lv）且字級提高。
  - `Mult` 在手機模式放大，已上桌文案精簡。
- 未解決問題:
  - 卡牌視覺質感仍偏平（距離 Balatro mobile 風格仍有落差）。
  - 拖曳/目標區互動反饋仍不夠強。
- 下一輪優先:
  - 強化卡牌材質層次（框線、陰影、色階）。
  - 增加拖曳目標高亮與放置成功反饋。
  - 進一步調整主次區塊視覺權重。

## 執行規範（每輪結束必做）
1. 更新本檔最新輪次紀錄。
2. 更新 `docs/systems/S10-ui-ux/SYSTEM.md` 的「更新紀錄」一行摘要。
3. 執行必要驗證（至少確認可編譯，若有測試則記錄 job id）。
4. `git add` 相關檔案、`git commit`、`git push`。
