# 10 - 執行期狀態與事件契約

## 1. 目的

統一 runtime 狀態結構與事件命名，避免 UI、邏輯、存檔互相耦合。

## 2. 核心狀態物件

### 2.1 RunState

```json
{
  "runId": "uuid",
  "phase": "Boot|RunStart|BlindStart|HandSelect|HandResolve|BlindResult|Shop|AnteAdvance|BossResolve|RunComplete|RunFail",
  "ante": 1,
  "blindType": "Small|Big|Boss",
  "targetScore": 250,
  "currentScore": 0,
  "playsLeft": 4,
  "discardsLeft": 3,
  "money": 8,
  "activeModifiers": ["MOD_HAND_LIMIT_4"],
  "seed": 123456
}
```

### 2.4 BlindResolution

```json
{
  "passed": true,
  "blindType": "Small|Big|Boss",
  "ante": 1,
  "currentScore": 260,
  "targetScore": 250,
  "nextPhase": "Shop|RunFail|RunComplete"
}
```

### 2.2 HandResolution

```json
{
  "handType": "GrammarChain",
  "baseChips": 30,
  "cardChips": 22,
  "baseMult": 4,
  "additiveMult": 2,
  "multipliers": [1.5, 1.1],
  "finalScore": 343,
  "wrongAnswers": 1
}
```

### 2.3 MetaSettlement

```json
{
  "xpGained": 120,
  "lpGainedBase": 14,
  "lpGainedContract": 5,
  "lpGainedTotal": 19,
  "contractCapApplied": false,
  "unlocks": ["LEX_T2"],
  "saveVersion": 2
}
```

## 3. 事件命名規則

格式：`domain.action.phase`

例：
- `run.start.requested`
- `run.start.succeeded`
- `blind.result.resolved`
- `hand.score.computed`
- `meta.settlement.persisted`

## 4. 事件表

| 事件 | 發送者 | 訂閱者 | 載荷 |
|---|---|---|---|
| `run.start.succeeded` | RunManagerV2 | UI, Telemetry | RunState |
| `hand.score.computed` | ScoringManagerV2 | RunManagerV2, UI | HandResolution |
| `learning.answer.applied` | LearningManagerV2 | UI, MetaManagerV2 | answer result |
| `shop.offers.generated` | ShopManagerV2 | Shop UI | offer list |
| `contract.settlement.done` | MetaManagerV2 | UI, Telemetry | MetaSettlement |
| `save.migration.failed` | MetaManagerV2 | UI, Logger | error payload |

## 5. 相依限制

- UI 只能讀公開 state，不可直接改 manager 私有欄位。
- Manager 間通訊優先使用事件與 service 介面。
- 禁止雙向事件循環（A 觸發 B，B 同步觸發 A）。

## 6. 版本管理

- 事件 payload 加 `schemaVersion`。
- 新增欄位可選（optional）優先，避免破壞舊 UI。
- 移除欄位需經 ADR 並標記 deprecate 一個版本週期。
