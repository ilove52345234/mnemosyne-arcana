# 14 - 核心用例序列圖（文字序列）

## 1. 用例 A：出牌結算

```text
UI -> RunManagerV2: SubmitHand(cards)
RunManagerV2 -> ScoringManagerV2: EvaluateHand(cards, modifiers)
ScoringManagerV2 -> LearningManagerV2: ApplyAnswerEffects(cards, answerResults)
LearningManagerV2 --> ScoringManagerV2: adjusted card values
ScoringManagerV2 --> RunManagerV2: ScoreBreakdown(finalScore)
RunManagerV2 -> EventBus: hand.score.computed
RunManagerV2 -> UI: RenderHandResult
```

## 2. 用例 B：盲注結算

```text
RunManagerV2: accumulate score
RunManagerV2 -> RuleEngine: CheckTarget(targetScore, currentScore)
RuleEngine --> RunManagerV2: pass/fail
RunManagerV2 -> EventBus: blind.result.resolved
RunManagerV2 -> ShopManagerV2: GenerateOffers() (if pass)
RunManagerV2 -> MetaManagerV2: SettleFailure() (if fail)
```

## 3. 用例 C：商店購買

```text
UI -> ShopManagerV2: Purchase(itemId)
ShopManagerV2 -> WalletService: SpendGold(cost)
WalletService --> ShopManagerV2: success/fail
ShopManagerV2 -> Inventory/DeckService: ApplyItemEffect
ShopManagerV2 -> EventBus: shop.purchase.completed
ShopManagerV2 -> UI: refresh offers and wallet
```

## 4. 用例 D：Run 結算寫檔

```text
RunManagerV2 -> MetaManagerV2: BuildRunResult
MetaManagerV2 -> ContractService: SettleContract
MetaManagerV2 -> ProgressService: CalculateXPAndLP
MetaManagerV2 -> SaveService: Persist(meta_progress, word_progress)
SaveService --> MetaManagerV2: persisted
MetaManagerV2 -> EventBus: meta.settlement.persisted
```

## 5. 失敗流程（migration）

```text
Boot -> SaveService: Load(save)
SaveService: version mismatch
SaveService -> MigrationService: Migrate(v1->v2)
MigrationService -> SaveService: fail
SaveService: restore backup
SaveService -> UI: show migration error and block run
```
