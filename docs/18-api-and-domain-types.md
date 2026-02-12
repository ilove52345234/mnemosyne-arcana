# 18 - API 與 Domain 型別契約

## 1. 目的

把 `03-technical-architecture.md` 的介面落到可編譯型別層，避免團隊各自定義 DTO。

## 2. 核心 DTO

### 2.1 PlayedCard

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| wordId | string | 是 | 對應詞庫 ID |
| element | Element | 是 | 元素 |
| partOfSpeech | PartOfSpeech | 是 | 詞性 |
| baseChips | int | 是 | 卡牌籌碼基值 |
| learningLevel | LearningLevel | 是 | Lv0~Lv4 |
| chipMultiplier | float | 否 | 單卡籌碼係數（預設 1.0） |
| isAnswerWrong | bool | 否 | 是否本手答錯（預設 false） |
| versionTags | string[] | 否 | 版本/詞綴標記 |

### 2.2 ScoreBreakdown

| 欄位 | 型別 | 說明 |
|---|---|---|
| handType | HandType | 判定牌型 |
| baseHandChips | int | 牌型基礎籌碼 |
| upgradedHandChips | int | 套用教材升級後籌碼 |
| cardChipsTotal | int | 單卡籌碼總和 |
| baseHandMult | int | 牌型基礎倍率 |
| upgradedHandMult | int | 套用教材升級後倍率 |
| additiveMultTotal | float | 加算倍率總和 |
| wrongAnswers | int | 本手答錯張數 |
| effectiveHandMult | int | 套懲罰與外部調整後倍率（最低 1） |
| multiplicativeFactors | float[] | 乘算因子 |
| finalScore | int | 最終得分 |

### 2.3 LearningResult

| 欄位 | 型別 | 說明 |
|---|---|---|
| isCorrect | bool | 是否答對 |
| questionMode | string | 題型（`4_choice_reading`/`2_choice_reading`/`2_choice_listening`/`spelling`/`auto`） |
| timeLimitSeconds | float | 該題型限時（秒） |
| chipMultiplier | float | 該卡籌碼係數 |
| handMultDelta | int | 牌型倍率增減 |
| nextLevel | LearningLevel | 更新後等級 |
| effectiveLevel | LearningLevel | 本題實際套用等級（Boss 可覆寫 Lv4->Lv3） |
| isAutoResolved | bool | 是否免答（Lv4 一般盲注） |
| decayUpdated | bool | 是否更新退化計時 |

### 2.4 ContractSettlement

| 欄位 | 型別 | 說明 |
|---|---|---|
| contractId | string | 契約 ID |
| completed | bool | 是否完成 |
| lpBonusRaw | int | 原始獎勵 |
| lpBonusCapped | int | 套上限後獎勵 |
| capApplied | bool | 是否觸發 45% 上限 |

### 2.5 MetaSettlement

| 欄位 | 型別 | 說明 |
|---|---|---|
| xpGained | int | 本局 XP |
| lpGainedBase | int | 基礎 LP |
| lpGainedContract | int | 契約 LP |
| lpGainedTotal | int | 合計 LP |
| unlockedNodes | string[] | 新解鎖節點 |
| unlockedLexiconTiers | string[] | 新詞庫層級 |

### 2.6 RunModifiers

| 欄位 | 型別 | 說明 |
|---|---|---|
| handUpgradeLevel | int | 教材對應的牌型升級等級（最低 0） |
| additiveMultTotal | float | 全域加算倍率 |
| handMultDelta | int | 外部調整（語感/事件） |
| multiplicativeFactors | float[] | 乘算因子陣列 |

### 2.7 RunContext

| 欄位 | 型別 | 說明 |
|---|---|---|
| ante | int | 當前 Ante |
| blindType | BlindType | 盲注類型 |
| playsLeft | int | 剩餘出牌次數 |
| discardsLeft | int | 剩餘棄牌次數 |
| currentLevel | LearningLevel | 目前單字等級 |
| consecutiveWrongCount | int | 連錯計數（保底機制輸入） |

### 2.8 ShopOffer

| 欄位 | 型別 | 說明 |
|---|---|---|
| offerId | string | 商品識別碼 |
| category | ShopOfferCategory | 商品類型（Sense/Material/Affix/Course） |
| price | int | 商品價格 |
| weight | int | 抽樣權重（除錯/平衡用途） |

### 2.9 PurchaseResult

| 欄位 | 型別 | 說明 |
|---|---|---|
| success | bool | 是否購買成功 |
| cost | int | 扣除成本 |
| remainingMoney | int | 購買後餘額 |
| offerId | string | 商品識別碼 |
| error | ErrorCode | 失敗錯誤碼（成功時為 `None`） |

### 2.10 WrongAnswerChoiceResult

| 欄位 | 型別 | 說明 |
|---|---|---|
| choice | WrongAnswerChoice | 玩家選擇（接受/重答/賭一把） |
| accepted | bool | 系統是否接受該選項 |
| retryConsumed | bool | 是否消耗本題重答機會 |
| moneySpent | int | 本次花費 |
| remainingMoney | int | 剩餘金錢 |
| finalAnswerResult | AnswerResult | 套用後答案結果 |
| overrideChipMultiplier | float | 覆寫卡牌籌碼係數（0.0/0.5/1.0） |

## 3. Enum 契約

```csharp
public enum Element { Life, Force, Mind, Matter, Abstract }
public enum PartOfSpeech { N, V, A, D }
public enum LearningLevel { Lv0, Lv1, Lv2, Lv3, Lv4 }
public enum BlindType { Small, Big, Boss }
public enum HandType { Word, PoSPair, ElemPair, PoSTriple, GrammarChain, ElemTriple, FullHouse, ElemFlush, PoSFlush, GrammarFlush }
public enum ShopOfferCategory { Sense, Material, Affix, Course }
public enum WrongAnswerChoice { AcceptLoss, RetryWithCost, Gamble }
public enum RunPhase { Boot, RunStart, BlindStart, HandSelect, HandResolve, BlindResult, Shop, AnteAdvance, BossResolve, RunComplete, RunFail }
```

## 4. Run 流程 DTO（M1-03）

### 4.1 BlindResolution

| 欄位 | 型別 | 說明 |
|---|---|---|
| passed | bool | 是否通關該盲注 |
| blindType | BlindType | 結算盲注類型 |
| ante | int | 當前 Ante |
| currentScore | int | 本盲注累積分 |
| targetScore | int | 目標分 |
| nextPhase | RunPhase | 結算後階段（`Shop`/`RunFail`/`RunComplete`） |

## 5. Nullability 與錯誤契約

- 所有 service 回傳 `Result<T, ErrorCode>` 風格（或等價模式）。
- 不允許以 `null` 表示業務錯誤。
- `ErrorCode` 最低集合：
  - `InvalidInput`
  - `ConfigMissing`
  - `StateConflict`
  - `PersistenceFailed`
  - `MigrationFailed`

## 6. 相容規則

1. DTO 新欄位優先 optional。
2. 破壞性欄位變更需更新 `saveVersion` 並補 migration。
3. Interface 改動需更新 `03`、`04`、`17` 三份文件。
