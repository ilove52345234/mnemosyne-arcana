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

### Round R18（2026-02-19）
- Screenshots:
  - `Assets/Screenshots/S10-auto-loop-r18-quiz-stage-animation-1.png`
- Score:
  - Layout: 27/30
  - Readability: 22/25
  - Interaction: 25/25
  - Polish: 15/20
  - Total: 89/100
- 本輪修改:
  - P0 主線：答題舞台加入進場淡入（`QuizFocusIn`）與每題切換脈衝動態。
  - 答題開始改為「進場動畫 -> 顯示第一題」，切題時焦點卡做輕量縮放/透明度轉場。
  - 答題重置與完成時加入動畫狀態回收，避免殘留轉場狀態。
- 未解決問題:
  - 出牌前導與翻牌揭露仍偏原型感，尚未接更完整的節奏動畫。
- 下一輪優先:
  - 補 `答題完成 -> 出卡` 的過渡過場（主 CTA 鎖定 + 中央提示動態）。

### Round R17（2026-02-19）
- Screenshots:
  - `Assets/Screenshots/S10-auto-loop-r17-horizontal-scrollbar-1.png`
- Score:
  - Layout: 27/30
  - Readability: 22/25
  - Interaction: 25/25
  - Polish: 14/20
  - Total: 88/100
- 本輪修改:
  - 新增主畫面「測試用橫向捲動軸（正式版移除）」。
  - 左右主區改包在 `ScrollRect` 內，支援橫向拖曳與底部 scrollbar 快速對位檢查。
  - 補上 content 最小寬度自適應，避免不同解析度下捲動失效。
- 未解決問題:
  - 目前為測試用途，正式版需移除該 scrollbar 與提示文案。
- 下一輪優先:
  - 在維持測試開關的前提下，調整底部手牌區視覺層次（框線/陰影/過場）。

### Round R16（2026-02-19）
- Screenshots:
  - `Assets/Screenshots/S10-auto-loop-r16-hand-bottom-1.png`
- Score:
  - Layout: 27/30
  - Readability: 22/25
  - Interaction: 24/25
  - Polish: 14/20
  - Total: 87/100
- 本輪修改:
  - 版面層級改為 Balatro 手機方向：手牌列下移到底部、牌桌區維持中段。
  - 不改互動邏輯（拖曳/點選/出牌流程維持），僅調整 UI sibling 順序。
- 未解決問題:
  - 目前只完成靜態佈局對齊，動畫節奏（手牌進場/出牌前導）仍需補齊。
- 下一輪優先:
  - 將手牌底部區加入更明確的視覺框與陰影層級。
  - 補答題 modal 與出牌前導動畫銜接。

### Round R15（2026-02-19）
- Screenshots:
  - `Assets/Screenshots/S10-auto-loop-r15-modal-1.png`
- Score:
  - Layout: 26/30
  - Readability: 22/25
  - Interaction: 24/25
  - Polish: 14/20
  - Total: 86/100
- 本輪修改:
  - 答題頁重構為中央 `modal` 舞台：題型/進度、焦點卡、題型容器、操作列收斂到同一區塊。
  - 新增舞台提示文案，降低主畫面與答題畫面的語義混淆。
  - 響應式新增 modal 高度與焦點卡高度/字級規則，改善手機橫向可讀性。
- 未解決問題:
  - modal 動態進出場動畫尚未加上，視覺節奏仍偏靜態。
- 下一輪優先:
  - 補 `modal` 進出場與題目切換動態（淡入 + 位移 + 卡片翻面前導）。
  - 把答題流程 CTA 層級再拉開（主按鈕更突出，返回按鈕次要化）。

### Round R14（2026-02-19）
- Screenshots:
  - `Assets/Screenshots/S10-auto-loop-r14-reveal-demo-1.png`
- Score:
  - Layout: 25/30
  - Readability: 21/25
  - Interaction: 24/25
  - Polish: 14/20
  - Total: 84/100
- 本輪修改:
  - M3.2：新增「一鍵演示翻牌」入口，強制走 `選牌 -> 答題 -> 出卡 -> 翻牌` 全流程。
  - 出卡後增加揭露停留視窗，提供穩定截圖時機。
  - 自動流程補強：非選項題型可自動提交，避免演示卡住。
- 未解決問題:
  - 目前仍是 Prototype 風格，與 Balatro 正式美術語言仍有質感落差。
- 下一輪優先:
  - 將「答題舞台」獨立成更清楚的中央模態層，降低主版面資訊壓力。
  - 針對橫向手機版補上進場/出牌過場動態，強化節奏感。

### Round R13（2026-02-19）
- Screenshots:
  - `Assets/Screenshots/S10-auto-loop-r10-m3-3.png`
- Score:
  - Layout: 25/30
  - Readability: 21/25
  - Interaction: 23/25
  - Polish: 14/20
  - Total: 83/100
- 本輪修改:
  - M3.1：翻牌可視強化（卡背尺寸、對比色、描邊、停留時間、CORRECT/WRONG 徽章）。
  - 自動流程補齊非選項題型（拼字/發音）自動提交，避免流程卡住。
- 未解決問題:
  - 截圖時機仍常落在「待命中」或答題過程，尚未穩定截到翻牌瞬間。
- 下一輪優先:
  - 新增「強制演示入口」按鈕，一鍵進入翻牌揭露並截圖。

### Round R12（2026-02-19）
- Screenshots:
  - `Assets/Screenshots/S10-auto-loop-r10-m3-1.png`
- Score:
  - Layout: 25/30
  - Readability: 21/25
  - Interaction: 23/25
  - Polish: 13/20
  - Total: 82/100
- 本輪修改:
  - 完成 M3 程式層：出卡協程改為逐張出卡，加入翻牌揭露時序節點。
  - 牌桌區生成雙面揭露卡內容（答案、正誤、`ART PLACEHOLDER`）。
  - `PlayCardsAnimationThenSubmit` 新增正誤旗標參數，支援答題結果映射到揭露卡。
- 未解決問題:
  - 目前翻牌可視強度偏弱，截圖不易看出翻面過程（需視覺強化）。
- 下一輪優先:
  - 增加翻牌視覺對比（卡背顏色、描邊、揭露停留時間）並補一張可視證據圖。

### Round R11（2026-02-19）
- Screenshots:
  - `Assets/Screenshots/S10-auto-loop-r10-m2-1.png`
- Score:
  - Layout: 25/30
  - Readability: 21/25
  - Interaction: 22/25
  - Polish: 12/20
  - Total: 80/100
- 本輪修改:
  - 完成 M2：中央焦點卡（放大單卡資訊）上線。
  - 題型容器切換上線：`中文選項 / 拼字(示意) / 發音(示意)`。
  - 答題提交流程抽象為 `SubmitQuizAnswer(bool)`，多題型可共用。
  - `ResetQuizState` 同步重置新題型容器與按鈕狀態。
- 未解決問題:
  - 發音與拼字仍為示意互動，尚未接真實輸入/音訊。
  - 卡片翻面揭露（答案 + 美術佔位）仍未完成視覺層。
- 下一輪優先:
  - M3：雙面卡翻轉、答案揭露欄位與美術佔位卡背。

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
