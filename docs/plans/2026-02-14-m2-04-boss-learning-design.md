# M2-04 Boss 學習規則設計

> 日期：2026-02-14
> 狀態：Approved

## 1. 概述

實作 Boss 盲注的三個學習機制：題型 +1 階、連對 3 題獎勵、全對升級獎勵。

## 2. 機制一：Boss 題型 +1 階

擴充現有 `GetEffectiveLevel`：

| 原始等級 | Boss 有效等級 |
|---|---|
| Lv0 | Lv1 |
| Lv1 | Lv2 |
| Lv2 | Lv3 |
| Lv3 | Lv3（封頂） |
| Lv4 | Lv3（已實作） |

## 3. 機制二：連對 3 題獎勵

- 每連續答對 3 題，下一張卡籌碼 x2
- 答錯重置計數
- 僅 Boss 盲注生效
- 純函式：`GetBossStreakBonus(consecutiveCorrect) → chipMultiplier`

## 4. 機制三：Boss 全對獎勵

- 條件：Boss 盲注所有答題皆正確
- 獎勵：本 Ante 打出的卡 +1 學習等級（上限 Lv4）
- 純函式：回傳升級清單，呼叫方負責套用

## 5. 新增 DTO

### BossStreakBonus

| 欄位 | 型別 | 說明 |
|---|---|---|
| consecutiveCorrect | int | 當前連對數 |
| chipMultiplier | float | 下一張卡的籌碼倍數（1.0 或 2.0） |

### BossRewardResult

| 欄位 | 型別 | 說明 |
|---|---|---|
| allCorrect | bool | 是否全對 |
| upgradedWords | List<(string wordId, LearningLevel from, LearningLevel to)> | 升級清單 |
| skippedAtMax | int | 已 Lv4 跳過的數量 |

## 6. 測試案例

| Case ID | 場景 | 預期 |
|---|---|---|
| TC-BOSS-001 | Boss + Lv0 | effectiveLevel = Lv1 |
| TC-BOSS-002 | Boss + Lv2 | effectiveLevel = Lv3 |
| TC-BOSS-003 | Boss + Lv3 | effectiveLevel = Lv3 |
| TC-BOSS-004 | Boss + Lv4 | effectiveLevel = Lv3 |
| TC-BOSS-005 | 連對 3 題 | chipMultiplier = 2.0 |
| TC-BOSS-006 | 連對 2 題後答錯 | 重置，chipMultiplier = 1.0 |
| TC-BOSS-007 | Boss 全對 | 打出卡各 +1 等級 |
| TC-BOSS-008 | Boss 全對但有 Lv4 | Lv4 不升，skippedAtMax = 1 |
| TC-BOSS-009 | 非 Boss 盲注 | 無連對獎勵 |
