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
