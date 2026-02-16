# 開發交接記錄

> 規則：每次工作結束，必須新增一筆記錄。

---

## 範本

### 交接記錄（YYYY-MM-DD）- 主題

- 目標：
- 完成內容：
- 變更檔案：
- 驗證結果：
- 風險/阻塞：
- 下一步：

---

## 交接記錄（2026-02-12）- 新專案文件體系建立

- 目標：建立可交接、可開發的規格與架構文檔體系
- 完成內容：
  - 建立 `README` 與 `docs/00~18` 主規格
  - 補齊 SA/SD 缺口（NFR、Runtime 契約、Risk、Context、Usecase、Balance SoT、Config 治理、Test Matrix、API 型別）
  - 建立 scripts/schema 基礎（config 驗證與測試入口）
- 變更檔案：
  - `README.md`
  - `docs/00-project-vision.md` ~ `docs/18-api-and-domain-types.md`
  - `scripts/*`
  - `docs/schemas/*`
- 驗證結果：文件存在、連結與目錄結構可讀
- 風險/阻塞：尚未進入 Unity 可執行專案骨架（M0）
- 下一步：開始 M0-01 / M0-02

## 交接記錄（2026-02-12）- M0 專案骨架完成（01~04）

- 目標：完成 M0 首批四項任務，建立可接手的最小開發入口
- 完成內容：
  - 建立 Unity 最小專案骨架（`Packages/manifest.json`、`ProjectSettings/ProjectVersion.txt`、`Assets/MnemosyneArcana/*`）
  - 建立五大 Manager V2 stubs 與核心 Domain/Service 契約型別
  - 建立 `configs/word_entries.v2.json`、`configs/meta_progress.v2.json` 範例資料並串接既有 config 驗證腳本
  - 建立 EditMode 測試入口（asmdef + `ManagerStubTests`）
- 變更檔案：
  - `Packages/manifest.json`
  - `ProjectSettings/ProjectVersion.txt`
  - `Assets/MnemosyneArcana/Scripts/**`
  - `Assets/MnemosyneArcana/Tests/EditMode/**`
  - `configs/word_entries.v2.json`
  - `configs/meta_progress.v2.json`
  - `README.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
- 驗證結果：
  - `bash scripts/validate_configs.sh` 通過
- 風險/阻塞：
  - 尚未在本機實跑 Unity batch test（需本機 Unity binary 路徑）
- 下一步：
  - 進入 M1-01：牌型判定引擎（先完成 `HandType` 判定 deterministic 測試）

## 交接記錄（2026-02-12）- M1-01 牌型判定引擎完成

- 目標：完成十種牌型判定邏輯，建立 deterministic 測試基線
- 完成內容：
  - `ScoringManagerV2` 新增由高到低牌型判定：`GrammarFlush -> PoSFlush -> ElemFlush -> FullHouse -> GrammarChain -> ElemTriple -> PoSTriple -> PoSPair -> ElemPair -> Word`
  - 實作語序鏈規則：`A -> N -> V -> D`，允許跳階與同詞性連續，逆序判定為失敗
  - 補上基礎計分組裝（SoT 基礎籌碼/倍率 + modifiers）與 deterministic 分數輸出
  - 新增 EditMode 測試案例（語序鏈、滿堂、語序同族、deterministic）
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Core/Managers/ScoringManagerV2.cs`
  - `Assets/MnemosyneArcana/Tests/EditMode/ScoringHandTypeTests.cs`
  - `Assets/MnemosyneArcana/Tests/EditMode/ManagerStubTests.cs`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
- 驗證結果：
  - `bash scripts/validate_configs.sh` 通過
  - Unity EditMode 測試在此環境受 Licensing IPC 阻塞（`LicenseClient` channel timeout）
- 風險/阻塞：
  - CI 或本機需可用 Unity 授權服務，否則無法完成 batchmode 測試
- 下一步：
  - M1-02：補齊牌型升級成長值（教材卡）與答錯懲罰整合（Learning/Scoring 邊界）

## 交接記錄（2026-02-12）- M1-02 分數公式與拆解完成

- 目標：完成可拆解、可驗證的得分公式，對齊 SoT 成長值與答錯懲罰規則
- 完成內容：
  - `ScoringManagerV2` 套入牌型升級成長值（`ChipsGrowth` / `MultGrowth`）
  - 答錯懲罰整合：答錯卡籌碼最多 50%，且每張答錯卡使牌型倍率 -1（最低 1）
  - 擴充 `ScoreBreakdown`：新增升級後籌碼/倍率、答錯張數、有效倍率欄位
  - 新增 `ScoringFormulaTests`，覆蓋：
    - 成長值套用
    - 答錯懲罰
    - 完整公式運算
  - 更新 `docs/17`、`docs/18`，同步最新型別與測試案例
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Core/Contracts/DomainModels.cs`
  - `Assets/MnemosyneArcana/Scripts/Core/Managers/ScoringManagerV2.cs`
  - `Assets/MnemosyneArcana/Tests/EditMode/ScoringFormulaTests.cs`
  - `docs/17-test-matrix.md`
  - `docs/18-api-and-domain-types.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
- 驗證結果：
  - `bash scripts/validate_configs.sh` 通過
- 風險/阻塞：
  - Unity batchmode 測試仍受授權服務限制，需在可授權環境跑完整 EditMode
- 下一步：
  - M1-03：Run/Blind 狀態機與通關/失敗流程

## 交接記錄（2026-02-12）- M1-03 盲注流程完成

- 目標：完成 Run 盲注流程的最小可運作狀態機（通關/失敗/推進）
- 完成內容：
  - `RunManagerV2` 新增流程方法：
    - `SubmitHandScore(int handScore)`
    - `ResolveBlindResult()`
    - `AdvanceAfterShop()`
  - 實作盲注目標分曲線（Ante 1~8；Small/Big/Boss）
  - 實作狀態轉移：
    - 達標 -> `Shop`
    - 未達標且出牌耗盡 -> `RunFail`
    - `Boss@Ante8` 達標 -> `RunComplete`
    - 商店後推進：`Small -> Big -> Boss -> 下一 Ante Small`
  - 新增 Runtime 契約：`RunPhase`、`BlindResolution`
  - 新增流程測試：`RunFlowTests`
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Core/Managers/RunManagerV2.cs`
  - `Assets/MnemosyneArcana/Scripts/Core/Runtime/RuntimeContracts.cs`
  - `Assets/MnemosyneArcana/Tests/EditMode/RunFlowTests.cs`
  - `docs/10-runtime-state-and-event-contracts.md`
  - `docs/17-test-matrix.md`
  - `docs/18-api-and-domain-types.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
- 驗證結果：
  - `bash scripts/validate_configs.sh` 通過
- 風險/阻塞：
  - Unity batchmode 測試在本環境仍受授權服務限制
- 下一步：
  - M1-04：商店進出與購買流程（先補 `ShopManagerV2` offer/purchase 契約）

## 交接記錄（2026-02-12）- M1-04 商店流程完成

- 目標：完成商店生成與購買最小流程，支撐 Run 迴圈的進出
- 完成內容：
  - `ShopManagerV2` 實作：
    - `GenerateOffers(ante, seed)`：5 格商品、deterministic 產生、Ante1 禁止課程卡
    - `PurchaseOffer(offer, currentMoney)`：扣款成功/餘額不足失敗
  - 新增商店 DTO：`ShopOffer`、`PurchaseResult`、`ShopOfferCategory`
  - 新增 EditMode 測試：`ShopManagerTests`（seed 決定論、Ante1 無課程卡、購買成功/失敗）
  - 更新 `docs/17` 與 `docs/18` 對應契約/測試案例
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Core/Contracts/DomainModels.cs`
  - `Assets/MnemosyneArcana/Scripts/Core/Managers/ShopManagerV2.cs`
  - `Assets/MnemosyneArcana/Tests/EditMode/ShopManagerTests.cs`
  - `docs/17-test-matrix.md`
  - `docs/18-api-and-domain-types.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
- 驗證結果：
  - `bash scripts/validate_configs.sh` 通過
- 風險/阻塞：
  - Unity batchmode 測試在本環境仍受授權限制，尚未跑完整 EditMode 測試集
- 下一步：
  - 進入 M2-01：Lv0~Lv4 行為模型（LearningManagerV2）

## 交接記錄（2026-02-12）- M2-01 Lv0~Lv4 行為模型完成

- 目標：將學習等級對應的題型/限時/籌碼係數與 Boss 特例落地
- 完成內容：
  - `LearningManagerV2.ApplyAnswer` 完成行為模型：
    - Lv0~Lv4 題型、限時、籌碼係數
    - `Boss + Lv4` 規則：以 Lv3 行為結算
    - `Wrong/GambleFailed` 輸出懲罰：`chipMultiplier=0.5`、`handMultDelta=-1`
    - `Correct/RetryAccepted/GambleSuccess` 視為答對並推進等級（上限 Lv4）
  - 擴充學習契約欄位：`questionMode`、`timeLimitSeconds`、`effectiveLevel`、`isAutoResolved`
  - 新增 `LearningManagerTests`（Lv0 答對、答錯懲罰、Boss Lv4 特例、賭一把成功）
  - 更新 `docs/17`、`docs/18` 的測試案例與 DTO 契約
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Core/Managers/LearningManagerV2.cs`
  - `Assets/MnemosyneArcana/Scripts/Core/Contracts/DomainModels.cs`
  - `Assets/MnemosyneArcana/Tests/EditMode/LearningManagerTests.cs`
  - `Assets/MnemosyneArcana/Tests/EditMode/ManagerStubTests.cs`
  - `docs/17-test-matrix.md`
  - `docs/18-api-and-domain-types.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
- 驗證結果：
  - `bash scripts/validate_configs.sh` 通過
- 風險/阻塞：
  - Unity batchmode 授權限制仍在，完整 EditMode 測試需在可授權環境執行
- 下一步：
  - M2-02：答錯三選一（接受/重答/賭一把）決策與成本規則

## 交接記錄（2026-02-12）- M2-02 答錯三選一完成

- 目標：把答錯後選擇機制落地為可重用服務 API
- 完成內容：
  - `ILearningService` 新增 `ResolveWrongAnswerChoice(...)`
  - `LearningManagerV2` 實作三選一：
    - 接受損失：免費，結果 `Wrong`，倍率 0.5
    - 重答：$2，單題一次，結果 `RetryAccepted`
    - 賭一把：以 seed 決定 50% 成功（1.0）/50% 失敗（0.0）
  - 新增 DTO / enum：`WrongAnswerChoice`、`WrongAnswerChoiceResult`
  - 補測試：接受損失、重答扣款與一次限制、賭一把 seed 決定論
  - 更新 `docs/17`、`docs/18` 與進度文件
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Core/Contracts/DomainModels.cs`
  - `Assets/MnemosyneArcana/Scripts/Core/Contracts/ServiceInterfaces.cs`
  - `Assets/MnemosyneArcana/Scripts/Core/Managers/LearningManagerV2.cs`
  - `Assets/MnemosyneArcana/Tests/EditMode/LearningManagerTests.cs`
  - `docs/17-test-matrix.md`
  - `docs/18-api-and-domain-types.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
- 驗證結果：
  - `bash scripts/validate_configs.sh` 通過
- 風險/阻塞：
  - Unity batchmode 授權限制仍在，完整 EditMode 測試尚未執行
- 下一步：
  - M2-03：退化規則（1/3/7 天）

## 交接記錄（2026-02-14）- M2-03 退化規則完成

- 目標：實作詞彙遺忘退化機制（1/3/7 天間隔）
- 完成內容：
  - 新增 `WordPool` enum、`WordProgress`、`DecayResult` DTO
  - 新增 `IDecayService` 介面（EvaluateDecay / EvaluateBatch / ResetDecayTimer）
  - 實作 `DecayManagerV2`：
    - Lv0 不退化
    - Lv1: 1 天 → Lv0, Decayed 池
    - Lv2: 3 天 → Lv1, Decayed 池
    - Lv3: 7 天 → Lv2, Decayed 池
    - Lv4: 7 天 → Lv3, Learning 池（不進 Decayed）
    - 邊界：>= 天數觸發退化
  - 9 個 EditMode 測試（TC-DECAY-001~007 + 2 邊界）
  - 更新 `docs/17`、`docs/18` 對應契約與測試案例
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Core/Contracts/DomainModels.cs`
  - `Assets/MnemosyneArcana/Scripts/Core/Contracts/ServiceInterfaces.cs`
  - `Assets/MnemosyneArcana/Scripts/Core/Managers/DecayManagerV2.cs`
  - `Assets/MnemosyneArcana/Tests/EditMode/DecayManagerTests.cs`
  - `docs/plans/2026-02-14-m2-03-decay-rules-design.md`
  - `docs/plans/2026-02-14-m2-03-decay-rules.md`
  - `docs/17-test-matrix.md`
  - `docs/18-api-and-domain-types.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
- 驗證結果：
  - Spec compliance review 通過
  - `bash scripts/validate_configs.sh` 通過
- 風險/阻塞：
  - Unity batchmode 授權限制仍在，完整 EditMode 測試需在可授權環境執行
- 下一步：
  - M2-04：Boss 關題型升級與全對獎勵

## 交接記錄（2026-02-14）- M2-04 Boss 學習規則完成

- 目標：實作 Boss 盲注的三個學習機制
- 完成內容：
  - 擴充 `GetEffectiveLevel`：Boss 時 Lv0→Lv1, Lv1→Lv2, Lv2→Lv3, Lv3→Lv3, Lv4→Lv3
  - 新增 `GetBossStreakBonus`：每連對 3 題下一張卡籌碼 x2
  - 新增 `ApplyBossAllCorrectReward`：Boss 全對打出卡 +1 等級（Lv4 上限跳過）
  - 新增 DTO：`BossStreakBonus`、`WordLevelUp`、`BossRewardResult`
  - 14 個 EditMode 測試（TC-BOSS-001~009 + 邊界）
  - 更新 `docs/17`、`docs/18` 對應契約與測試案例
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Core/Contracts/DomainModels.cs`
  - `Assets/MnemosyneArcana/Scripts/Core/Contracts/ServiceInterfaces.cs`
  - `Assets/MnemosyneArcana/Scripts/Core/Managers/LearningManagerV2.cs`
  - `Assets/MnemosyneArcana/Tests/EditMode/BossLearningTests.cs`
  - `docs/plans/2026-02-14-m2-04-boss-learning-design.md`
  - `docs/plans/2026-02-14-m2-04-boss-learning.md`
  - `docs/17-test-matrix.md`
  - `docs/18-api-and-domain-types.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
- 驗證結果：
  - Spec compliance review 通過
- 風險/阻塞：
  - Unity batchmode 授權限制仍在
- 下一步：
  - M2 全部完成，進入 M3-01：XP/LP 結算

## 交接記錄（2026-02-14）- M3-01~03 XP/LP 結算、契約系統、LP 上限守門完成

- 目標：實作局外迴圈核心三件：XP/LP 結算公式、契約生成與結算、LP 45% 上限
- 完成內容：
  - 擴充 `Contract` DTO：新增 `ContractType`、`Tier`、`LpReward` 欄位
  - 擴充 `RunTelemetry` DTO：新增 `ContractCompleted` 欄位
  - `IContractService` 新增 `SettleContractWithCap(contract, telemetry, lpBase)` 方法
  - `MetaManagerV2` 完整實作：
    - `SettleRun`：XP = Ante * 20 + (Clear ? 50 : 0)，LP = Ante * 2 + (Clear ? 5 : 0)
    - `GenerateContracts`：11 種契約池，seed 決定論，每次選 3 張
    - `SettleContract`：完成回傳原始 LP，未完成回傳 0
    - `SettleContractWithCap`：LP 上限 = floor(lpBase * 45 / 55)
  - 新增 `MetaManagerTests`：13 個測試案例覆蓋結算、契約生成、上限
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Core/Contracts/DomainModels.cs`
  - `Assets/MnemosyneArcana/Scripts/Core/Contracts/ServiceInterfaces.cs`
  - `Assets/MnemosyneArcana/Scripts/Core/Managers/MetaManagerV2.cs`
  - `Assets/MnemosyneArcana/Tests/EditMode/MetaManagerTests.cs`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - 自我審查通過：DTO 擴展向後相容、介面新增方法已由實作類滿足、LP cap 公式 45/55 正確
- 風險/阻塞：
  - Unity batchmode 授權限制仍在，完整 EditMode 測試需在可授權環境執行
- 下一步：
  - M3-04：課程樹 MVP 串接

## 交接記錄（2026-02-14）- 全專案盤點與文件一致性修正

- 目標：接手前一位進度，完成全專案分析並修正文件語言與進度一致性
- 完成內容：
  - 盤點本地與遠端差異：本地 `ahead 4`（M3-01~03 程式提交）
  - 將 3 份英文 `docs/plans` 實作計畫改寫為繁中版本：
    - `2026-02-14-m2-03-decay-rules.md`
    - `2026-02-14-m2-04-boss-learning.md`
    - `2026-02-14-m3-01-03-meta-progression.md`
  - 新增全專案分析文件：`docs/20-project-analysis-2026-02-14.md`
  - 同步更新階段文件：`README.md`、`docs/PROJECT_EXECUTION_PLAN.md`
  - 風險清單補充：Unity 授權阻塞與規格漂移風險（R-007、R-008）
- 變更檔案：
  - `README.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
  - `docs/11-risk-register-and-decision-log.md`
  - `docs/20-project-analysis-2026-02-14.md`
  - `docs/plans/2026-02-14-m2-03-decay-rules.md`
  - `docs/plans/2026-02-14-m2-04-boss-learning.md`
  - `docs/plans/2026-02-14-m3-01-03-meta-progression.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - 文件可讀性檢查完成，`docs/plans` 英文計畫已移除
- 風險/阻塞：
  - Unity batchmode 授權限制仍在（尚未可在本環境跑完整 EditMode）
- 下一步：
  - 提交並推送目前差異後，直接進入 M3-04（課程樹 MVP）

## 交接記錄（2026-02-14）- M3-04 課程樹 MVP 串接完成

- 目標：完成課程樹解鎖核心規則，讓局外迴圈在 M3 階段閉環
- 完成內容：
  - `MetaProgress` 新增 `CurriculumNodes`
  - `UnlockResult` 新增 `spentLp`、`remainingLp`、`error`、`unlockedNodes`
  - `MetaManagerV2.TryUnlockNode` 完成：
    - 節點存在檢查
    - 已解鎖檢查
    - 前置節點檢查（any-of group）
    - 互斥節點檢查
    - LP 成本檢查與扣除結果輸出
  - MVP 節點池先落地 4 分支前 3 層（含 A/B 互斥）
  - `MetaManagerTests` 新增 5 個課程樹測試（成功/前置不足/互斥/LP不足/已解鎖）
  - 同步更新 `README`、`docs/17`、`docs/18`、`docs/IMPLEMENTATION_STATUS`、`docs/PROJECT_EXECUTION_PLAN`
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Core/Contracts/DomainModels.cs`
  - `Assets/MnemosyneArcana/Scripts/Core/Managers/MetaManagerV2.cs`
  - `Assets/MnemosyneArcana/Tests/EditMode/MetaManagerTests.cs`
  - `README.md`
  - `docs/17-test-matrix.md`
  - `docs/18-api-and-domain-types.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - `bash scripts/validate_configs.sh` 通過
- 風險/阻塞：
  - 課程樹目前為 MVP（前 3 層），完整 4x12 仍待擴展
  - Unity batchmode 授權限制仍在，完整測試需在可授權環境執行
- 下一步：
  - M4-01：詞庫內容填充（T1/T2 可玩內容）

## 交接記錄（2026-02-14）- M4-01 詞庫內容填充完成

- 目標：建立 T1/T2 可玩詞庫基線，支撐 M4 平衡迭代
- 完成內容：
  - `configs/word_entries.v2.json` 擴充至 100 筆
  - T1/T2 各 50 筆，元素五系各 20，詞性 N/V/A/D 分布達最低門檻
  - `baseChips` 依單字長度規則映射（3/4/5/6/7+ 字母）
  - `scripts/validate_configs.py` 加入詞庫品質守門：
    - 總數、tier 數量、詞性覆蓋、元素覆蓋
    - difficulty 與 baseChips 合法範圍
  - `docs/16-config-governance.md` 補上 M4-01 基線規則
- 變更檔案：
  - `configs/word_entries.v2.json`
  - `scripts/validate_configs.py`
  - `docs/16-config-governance.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - `bash scripts/validate_configs.sh` 通過
  - 統計：total=100、T1=50、T2=50、元素各 20、詞性最低 20
- 風險/阻塞：
  - 詞義與難度仍需後續教學設計審核（語意一致性）
- 下一步：
  - M4-02：商店池權重與價格帶調整（搭配新詞庫做平衡）

## 交接記錄（2026-02-14）- M4-02 商店權重與價格帶調整完成

- 目標：讓商店生成規則符合分段平衡策略與 Boss 課程卡規格
- 完成內容：
  - `ShopManagerV2.GenerateOffers` 新增 `isBossShop` 參數（預設 `false`）
  - 實作 Ante 分段權重：
    - Ante 1-2：Material/Affix 偏高，Course 關閉
    - Ante 3-5：Material/Sense 偏高，Course 關閉
    - Ante 6-8：完整池，Course 低機率
  - 實作 Boss 商店：固定課程卡 2 選 1，價格固定 `$10`
  - 補測試：
    - Boss 商店回傳 2 張課程卡
    - 價格帶合法性（Sense 4-8 / Material 3-6 / Affix 2-4 / Course 10）
  - 同步更新 `docs/15`、`docs/17`、`docs/IMPLEMENTATION_STATUS`、`docs/PROJECT_EXECUTION_PLAN`
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Core/Managers/ShopManagerV2.cs`
  - `Assets/MnemosyneArcana/Tests/EditMode/ShopManagerTests.cs`
  - `docs/15-balance-source-of-truth.md`
  - `docs/17-test-matrix.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - `bash scripts/validate_configs.sh` 通過
- 風險/阻塞：
  - 尚未在可授權環境跑完整 Unity EditMode（現環境授權限制）
- 下一步：
  - M4-03：盲注曲線平衡與體感調整

## 交接記錄（2026-02-14）- M4-03 盲注曲線平衡完成

- 目標：在不破壞 SoT 標準曲線下，補上可調的體感檔位
- 完成內容：
  - 新增 `RunDifficultyProfile`（Relaxed / Standard / Challenging）
  - `RunManagerV2` 支援難度檔位：
    - `Standard`：使用 SoT 基線
    - `Relaxed`：前期目標分降低，後期回歸基線
    - `Challenging`：前期目標分提高，中後期維持高壓
  - `RunState` 新增 `difficultyProfile` 追蹤執行期檔位
  - `RunFlowTests` 新增 3 案：
    - Standard 基線值
    - Relaxed < Standard
    - Challenging > Standard
  - 更新 `docs/10`、`docs/15`、`docs/17`、`docs/18`、`docs/IMPLEMENTATION_STATUS`、`docs/PROJECT_EXECUTION_PLAN`
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Core/Contracts/DomainModels.cs`
  - `Assets/MnemosyneArcana/Scripts/Core/Managers/RunManagerV2.cs`
  - `Assets/MnemosyneArcana/Scripts/Core/Runtime/RuntimeContracts.cs`
  - `Assets/MnemosyneArcana/Tests/EditMode/RunFlowTests.cs`
  - `docs/10-runtime-state-and-event-contracts.md`
  - `docs/15-balance-source-of-truth.md`
  - `docs/17-test-matrix.md`
  - `docs/18-api-and-domain-types.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - `bash scripts/validate_configs.sh` 通過
- 風險/阻塞：
  - 仍需在可授權環境跑完整 Unity EditMode 測試
- 下一步：
  - M4-04：首輪平衡報告（彙整 M4-01~03 的數值與測試結論）

## 交接記錄（2026-02-14）- M4-04 首輪平衡報告完成

- 目標：產出 M4 首輪平衡總結，作為進入 Alpha Gate 的決策依據
- 完成內容：
  - 新增 `docs/21-m4-first-balance-report.md`
  - 彙整 M4-01~03 的可量化結果：
    - 詞庫分布與覆蓋率
    - 商店權重與價格帶
    - 盲注曲線三檔體感策略
  - 列出 Alpha 前風險與建議執行順序
  - 更新進度看板：M4 全完成，Alpha Gate 啟動
- 變更檔案：
  - `docs/21-m4-first-balance-report.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - `bash scripts/validate_configs.sh` 通過
- 風險/阻塞：
  - Unity batchmode 授權限制仍在，A-01 需在可授權環境執行
- 下一步：
  - A-01：Ante 1-8 全流程回歸

## 交接記錄（2026-02-16）- 文件狀態一致性修正（docs/20）

- 目標：修正 `docs/20` 與看板狀態不一致問題，建立單一可信狀態
- 完成內容：
  - 更新 `docs/20-project-analysis-2026-02-14.md` 內容至最新狀態：
    - `M3` / `M4` 由 Todo 改為 Done
    - `Alpha Gate` 改為 In Progress
    - 移除已完成缺口（如 `TryUnlockNode` 未實作）
    - 將下一步改為 `A-01~A-04` 驗收路徑
- 變更檔案：
  - `docs/20-project-analysis-2026-02-14.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - 與 `docs/IMPLEMENTATION_STATUS.md`、`docs/PROJECT_EXECUTION_PLAN.md` 對齊
- 風險/阻塞：
  - 無新增阻塞（維持現有 Unity 授權限制）
- 下一步：
  - 持續 A-01 回歸，完成後同步更新 `docs/20` 與看板
