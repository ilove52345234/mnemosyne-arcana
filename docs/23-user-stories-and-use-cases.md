# 23 - Alpha 可玩流程 User Stories 與 Use Cases

> 日期：2026-02-17  
> 目的：先確保產品可完整遊玩一輪（Run Start -> Run Complete + Meta 結算），再進入 A-02 存檔/migration。

## 1. User Stories

### US-01 開局可進入可操作狀態
- 身為玩家，我在 Editor 任何 Scene 按下 Play 後，應該立即看到可操作的原型 UI。
- 驗收：Play Mode 進入後存在 `PrototypeCardGameUiController` 與 UI 物件。

### US-02 單手流程可計分並推進
- 身為玩家，我可以抽牌、答題、出牌，並看到分數累加與盲注進度更新。
- 驗收：`HandSelect -> HandResolve -> BlindResult` 可正確轉移。

### US-03 盲注結果可分流
- 身為玩家，我在達標時會進商店，未達標且耗盡出牌次數時會失敗。
- 驗收：`BlindResult -> Shop`（pass）與 `BlindResult -> RunFail`（fail）都成立。

### US-04 商店可生成並購買
- 身為玩家，我可以看到商品並購買，金幣會正確扣除，餘額不足時會被拒絕。
- 驗收：Offer 生成、成功購買、失敗購買三條路徑皆可驗證。

### US-05 關卡可完整推進到通關
- 身為玩家，我可以從 Ante1 一路推進到 Ante8 Boss 並結束本局。
- 驗收：`RunComplete` 路徑可重現，且不出現死狀態。

### US-06 通關後可進入 Meta 結算
- 身為玩家，我通關後可看到 XP/LP 結算與契約獎勵邊界（45% cap）被守住。
- 驗收：`SettleRun` 與 `SettleContractWithCap` 輸出合法。

## 2. Use Cases

| Use Case ID | 對應故事 | 主要流程 | 驗證方式 |
|---|---|---|---|
| UC-01 | US-01 | Play Mode 啟動 -> 自動建立原型 UI | Unity MCP（Play + hierarchy/search） |
| UC-02 | US-02 | 答題結果套用 -> 出牌計分 -> 提交手牌 | EditMode 測試 `PlayableLoopUseCaseTests` |
| UC-03 | US-03 | 盲注結算 pass/fail 分流 | 既有 `RunFlowTests` + `AlphaRegressionTests` |
| UC-04 | US-04 | 商店生成 -> 購買 -> 扣款或拒絕 | 既有 `ShopManagerTests` + 新 use case 測試 |
| UC-05 | US-05 | Ante1~8 完整迴圈 | `AlphaRegressionTests` + 新 use case 測試 |
| UC-06 | US-06 | Run 結算 + 契約 cap | `MetaManagerTests` + 新 use case 測試 |

## 3. 完成定義（本階段）

1. UC-01~UC-06 全部驗證通過。
2. Unity MCP 可重現 Play Mode 原型 UI 存在。
3. EditMode 測試全綠，且新增 use case 測試全綠。
4. 再進入 A-02（存檔/migration）實作。

## 4. 驗收測試實作（2026-02-17）

- `Assets/MnemosyneArcana/Tests/EditMode/UserStoryAcceptanceTests.cs`
- US-01：`US01_PlayStartsWithPrototypeUi`
- US-02：`US02_SingleHandCanScoreAndMoveToBlindResult`
- US-03：`US03_BlindCanPassOrFail`
- US-04：`US04_ShopCanGenerateAndPurchaseWithBalanceGuard`
- US-05：`US05_CanCompleteFullAnte1To8Run`
- US-06：`US06_MetaSettlementAndContractCapAreValid`

## 5. 第二批故事（2026-02-17）

### US-07 失敗後可重開新局
- 驗收：`US07_CanRestartRunAfterFailure`

### US-08 Boss 商店固定課程二選一
- 驗收：`US08_BossShopAlwaysOffersTwoCoursesAtPrice10`

### US-09 答錯三選一可用且守門正確
- 驗收：`US09_WrongAnswerThreeChoicesWorkAsDesigned`

## 6. 第三批故事（2026-02-17）

### US-10 契約生成可重現，且支援一次刷新
- 驗收：`US10_ContractGenerationIsDeterministicAndSupportsSingleRefresh`

### US-11 課程樹前置與互斥守門有效
- 驗收：`US11_CurriculumNodeMutexAndPrereqAreEnforced`

### US-12 Boss 學習加成與全對獎勵有效
- 驗收：`US12_BossLearningBoostAndAllCorrectRewardWork`
