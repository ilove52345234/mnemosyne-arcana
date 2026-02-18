# S3 - Learning（答題、三選一、Boss）

## 1. 設計規劃
- 目標：學習嵌在玩法內，不做跳窗式中斷。
- 核心原則：答錯降收益，不破壞組牌成立。

## 2. 規格文件
- 等級行為：Lv0(4選1) -> Lv1(2選1) -> Lv2(2選1聽力) -> Lv3(拼字) -> Lv4(免答)。
- 答錯後三選一：
- AcceptLoss：免費接受降益
- RetryWithCost：花費重答（單題一次）
- Gamble：50% 回復 / 50% 歸零
- 保底：連錯 3 題降難，連錯 5 題該關剩餘題型進一步降難。
- Boss 規則：
- 題型整體 +1 階
- Lv4 在 Boss 以 Lv3 行為處理
- 每連對 3 題下一張卡 x2
- Boss 全對後，當 Ante 打出卡可 +1 等級（有上限）

## 3. 實作紀錄
- 已完成 Lv0~Lv4 行為模型。
- 已完成答錯三選一決策 API。
- 已完成 Boss 升階、連對獎勵與全對升級。

## 4. 驗測報告與調整建議
- 驗測結論（2026-02-18）：`Done`（符合當前 S3 門檻）。
- 三模型對應：
  - `M-Low`：`LearningManagerTests.ApplyAnswer_Lv3Wrong_AppliesPenaltyAndNoLevelUp` + `ResolveWrongAnswerChoice_AcceptLoss_KeepsMoneyAndPenalty`。
  - `M-Mid`：`LearningManagerTests.ApplyAnswer_Lv0Correct_UsesLv0BehaviorAndLevelsUp` + `...RetryWithCost_SpendsTwo`。
  - `M-High`：`BossLearningTests.Boss_Lv2_EffectiveLv3` + `BossAllCorrect_UpgradesPlayedWords` + `US12_BossLearningBoostAndAllCorrectRewardWork`。
- 失敗/邊界案例：
  - `LearningManagerTests.ApplyAnswer_EmptyWordId_ReturnsInvalidInput`
  - `LearningManagerTests.ResolveWrongAnswerChoice_RetryUsed_ReturnsStateConflict`
  - `BossLearningTests.BossAllCorrect_NullInput_ReturnsError`
- 證據（MCP job）：
  - `50345a33123740d18bd0d0e337af7a50`（LearningManagerTests：9/9）
  - `7251ce2f9e3a4b5aafd8eb9b976f1d52`（BossLearningTests：14/14）
  - `1d2d6da66d75418687e8d6401009feb3`（US12：1/1）
- 調整建議（小幅）：
1. 若新手連錯率過高，可先提高 Lv1/Lv2 題型寬限（+0.2s）而不是降低懲罰倍率。
2. 若 Boss 關難度突刺，優先調整「連對 x2 觸發頻率」或 Boss 題型升階比例，不動核心等級規則。
3. 維持「答錯不斷牌型」與「三選一可逆」兩個設計支柱不變。

## 5. 更新紀錄
- 2026-02-18：改為系統自洽文件，不再使用跨文件引用描述。
- 2026-02-18：完成 S3 首輪行為驗測，達成 Done 門檻（Low/Mid/High + boundary case + job evidence）。
