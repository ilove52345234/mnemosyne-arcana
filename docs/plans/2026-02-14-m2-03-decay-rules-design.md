# M2-03 退化規則設計

> 日期：2026-02-14
> 狀態：Approved

## 1. 概述

實作詞彙遺忘退化機制。根據學習等級對應不同退化間隔（1/3/7 天），未在期限內練習的詞彙將降級並移入退化池。

## 2. 退化間隔對照

| 等級 | 不練習天數 | 退化目標等級 | 退化目標池 |
|---|---|---|---|
| Lv0 | — | 不退化 | — |
| Lv1 | 1 天 | Lv0 | Decayed |
| Lv2 | 3 天 | Lv1 | Decayed |
| Lv3 | 7 天 | Lv2 | Decayed |
| Lv4 | 7 天 | Lv3 | Learning（再觸發 Lv3 的 7 天計時） |

## 3. 資料模型

### 3.1 WordProgress

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| wordId | string | 是 | 對應 WordEntryV2.id |
| level | LearningLevel | 是 | 當前學習等級 |
| pool | WordPool | 是 | 所屬池 |
| lastPracticed | DateTime | 是 | 最後練習時間（UTC） |

### 3.2 WordPool enum

`Locked | Discoverable | Learning | Mastered | Decayed`

### 3.3 DecayResult

| 欄位 | 型別 | 說明 |
|---|---|---|
| wordId | string | 詞彙 ID |
| decayed | bool | 是否退化 |
| previousLevel | LearningLevel | 退化前等級 |
| newLevel | LearningLevel | 退化後等級 |
| previousPool | WordPool | 退化前池 |
| newPool | WordPool | 退化後池 |

## 4. 介面設計

### 4.1 IDecayService

```csharp
public interface IDecayService
{
    DecayResult EvaluateDecay(WordProgress word, DateTime now);
    IReadOnlyList<DecayResult> EvaluateBatch(IReadOnlyList<WordProgress> words, DateTime now);
    void ResetDecayTimer(WordProgress word, DateTime now);
}
```

### 4.2 DecayManagerV2

純函式實作，無副作用。`EvaluateDecay` 只回傳判定結果，不改寫 WordProgress 本身。
呼叫方根據 `DecayResult` 決定是否套用。

## 5. 整合點

- `LearningManagerV2.ApplyAnswer`：答對時呼叫 `ResetDecayTimer`
- Run 啟動前（Boot 階段）：呼叫 `EvaluateBatch` 批次更新池狀態
- 退化結果寫回 `WordProgress` 由呼叫方負責（不在 DecayService 內）

## 6. 測試案例

| Case ID | 場景 | 預期 |
|---|---|---|
| TC-DECAY-001 | Lv1 + 超過 1 天未練 | 退化到 Lv0, Decayed 池 |
| TC-DECAY-002 | Lv2 + 剛好 3 天未練 | 退化到 Lv1, Decayed 池 |
| TC-DECAY-003 | Lv3 + 6 天未練 | 不退化 |
| TC-DECAY-004 | Lv4 + 超過 7 天 | 退化到 Lv3, Learning 池 |
| TC-DECAY-005 | Lv0 + 任何時間 | 不退化 |
| TC-DECAY-006 | 答對後重設計時 | lastPracticed 更新 |
| TC-DECAY-007 | 批次退化多詞 | 各詞獨立判定 |

## 7. 邊界條件

- `lastPracticed` 為 `DateTime.MinValue` 時：視為從未練習，立即退化
- 退化判定使用 `>=`（等於天數也觸發退化）
- Lv4 退化到 Lv3 後不進 Decayed 池，留在 Learning 池
- Lv0 永不退化（已是最低等級）
