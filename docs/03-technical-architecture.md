# 03 - 技術架構規格

## 1. 技術基線

- 引擎：Unity + C#
- 設定：JSON 資料驅動
- 原則：Domain logic 可在純 C# 測試

## 2. 模組邊界與責任

### 2.1 RunManagerV2
責任：
- 管控 Run 狀態機
- 管控盲注進度、目標分、通關/失敗
- 串接 Shop 與結算流程

不負責：
- 分數計算細節
- 學習升降級細節

### 2.2 ScoringManagerV2
責任：
- 牌型判定
- 籌碼/倍率組裝
- 乘算連乘結果輸出

不負責：
- UI 顯示
- 存檔

### 2.3 LearningManagerV2
責任：
- 題型選擇與答題結果應用
- Lv0-4 升降級
- 遺忘曲線與退化更新

### 2.4 ShopManagerV2
責任：
- 商品池抽樣
- 價格與重擲規則
- 養牌區固定槽維護

### 2.5 MetaManagerV2
責任：
- XP/LP 結算
- 課程樹解鎖
- 契約生成/驗收
- 詞庫層級解鎖
- 存檔讀寫

## 3. 核心介面（最小集合）

```csharp
public interface IScoringService
{
    ScoreBreakdown EvaluateHand(IReadOnlyList<PlayedCard> cards, RunModifiers modifiers);
}

public interface ILearningService
{
    LearningResult ApplyAnswer(WordId wordId, AnswerResult answer, RunContext runContext);
}

public interface IContractService
{
    IReadOnlyList<Contract> GenerateContracts(MetaProgress meta, int seed);
    ContractSettlement SettleContract(Contract contract, RunTelemetry telemetry);
}

public interface IMetaProgressService
{
    MetaSettlement SettleRun(RunResult runResult, MetaProgress current);
    UnlockResult TryUnlockNode(string nodeId, MetaProgress current);
}
```

## 4. Run 狀態機

狀態：
1. Boot
2. RunStart
3. BlindStart
4. HandSelect
5. HandResolve
6. BlindResult
7. Shop
8. AnteAdvance
9. BossResolve
10. RunComplete
11. RunFail

主要轉移：
- BlindStart -> HandSelect
- HandSelect -> HandResolve
- HandResolve -> BlindResult
- BlindResult -> Shop（通關）或 RunFail（失敗）
- Shop -> AnteAdvance 或 BlindStart
- Ante 8 Boss 通關 -> RunComplete

## 5. 資料流（單手）

1. UI 提交打牌
2. ScoringManager 判定牌型與基礎分
3. LearningManager 套用答題修正
4. RunManager 寫入盲注進度
5. 事件系統推送 UI 顯示與戰報

## 6. 場景與 UI 流程

建議場景：
- `Bootstrap`
- `RunScene`
- `MetaScene`

`RunScene` 內 UI 子層：
- HandPanel
- BlindPanel
- QuizOverlay
- ShopPanel
- SettlementPanel

## 7. 錯誤處理與恢復

1. 設定檔缺失
- 使用預設值 + 記錄 error log + 阻擋進入正式 Run

2. 存檔版本不符
- 執行 migration
- migration 失敗則回退備份並提示

3. 無效牌型/資料
- 該手作廢為最低分，不崩潰整局

4. 契約結算異常
- 套用安全預設（無額外 LP）

## 8. 測試策略

必要單元測試：
- 牌型判定一致性
- 分數公式 deterministic
- 契約獎勵 <=45% 邊界
- 詞庫解鎖門檻判定
- 存檔 migration 正確性

必要整合測試：
- Ante 1->8 流程可跑完
- Shop/Settlement 不產生死狀態
