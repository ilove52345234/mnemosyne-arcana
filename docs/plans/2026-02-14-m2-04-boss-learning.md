# M2-04 Boss 學習規則實作計畫（已完成）

> 日期：2026-02-14
> 狀態：Done

## 1. 目標

完成 Boss 盲注學習機制：題型升階、連對獎勵、全對升級回饋。

## 2. 實作摘要

- `LearningManagerV2.GetEffectiveLevel` 套入 Boss 升階規則。
- 新增連對獎勵 API：`GetBossStreakBonus(consecutiveCorrect)`。
- 新增全對回饋 API：`ApplyBossAllCorrectReward(playedWords)`。
- 新增 DTO：
- `BossStreakBonus`
- `WordLevelUp`
- `BossRewardResult`

## 3. 規則落地

- Boss 關題型升階：低等級在 Boss 會提升挑戰強度。
- 每連對 3 題：下一張卡獲得 `x2` 籌碼倍率。
- Boss 全對：本 Ante 打出詞卡可升 1 級（`Lv4` 不再提升）。

## 4. 測試覆蓋

- `BossLearningTests`：
- Boss 升階行為
- 連對倍率門檻
- 全對升級與上限

## 5. 交付檔案

- `Assets/MnemosyneArcana/Scripts/Core/Contracts/DomainModels.cs`
- `Assets/MnemosyneArcana/Scripts/Core/Contracts/ServiceInterfaces.cs`
- `Assets/MnemosyneArcana/Scripts/Core/Managers/LearningManagerV2.cs`
- `Assets/MnemosyneArcana/Tests/EditMode/BossLearningTests.cs`

## 6. 後續銜接

- `RunManagerV2` 在 Boss hand resolve 後串接連對計數。
- M3 契約可直接使用 `BossRewardResult` 做條件判斷。
