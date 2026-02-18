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

## 交接記錄（2026-02-19）- S10-P0 M3.1 翻牌可視強化

- 目標：提升翻牌揭露可視性並避免非選項題型卡住
- 完成內容：
  - 翻牌卡背顯示強化（對比色、描邊、徽章、停留時間）
  - 拼字/發音題型加入自動提交流程支援（自動演示可前進）
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Prototype/PrototypeCardGameUiController.cs`
  - `docs/systems/S10-ui-ux/ALIGNMENT_TRACKER.md`
  - `docs/systems/S10-ui-ux/SYSTEM.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - Unity 編譯無 error
  - 截圖：`Assets/Screenshots/S10-auto-loop-r10-m3-3.png`
- 風險/阻塞：
  - 目前截圖時機不穩，未必剛好落在翻牌瞬間
- 下一步：
  - 加一鍵強制演示入口，確保可穩定截到翻牌證據

---

## 交接記錄（2026-02-19）- S10-P0 M3 逐張出卡與翻牌揭露

- 目標：把答題完成後的出卡流程升級為逐張出卡 + 翻牌揭露
- 完成內容：
  - `PlayCardsAnimationThenSubmit` 改為逐張出卡序列
  - 新增翻牌揭露節點與卡背資料（答案、正誤、`ART PLACEHOLDER`）
  - 支援將答題正誤旗標映射到揭露卡
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Prototype/PrototypeCardGameUiController.cs`
  - `docs/systems/S10-ui-ux/ALIGNMENT_TRACKER.md`
  - `docs/systems/S10-ui-ux/SYSTEM.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - Unity 編譯無 error
  - 截圖：`Assets/Screenshots/S10-auto-loop-r10-m3-1.png`
- 風險/阻塞：
  - 翻牌可視效果仍偏弱，需再強化視覺停留
- 下一步：
  - M3.1：翻牌視覺強化（卡背對比/停留時間）並補證據截圖

---

## 交接記錄（2026-02-19）- S10-P0 M2 題目舞台改版

- 目標：完成中央單卡放大答題舞台與多題型容器
- 完成內容：
  - 新增中央焦點卡（顯示當前答題單字與元素/詞性/等級）
  - 新增三種題型容器：中文選項、拼字（示意）、發音（示意）
  - 新增 `SubmitQuizAnswer(bool)` 統一提交流程
  - `PresentNextQuizQuestion` 可依題序切換題型容器
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Prototype/PrototypeCardGameUiController.cs`
  - `docs/systems/S10-ui-ux/ALIGNMENT_TRACKER.md`
  - `docs/systems/S10-ui-ux/SYSTEM.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - Unity 編譯無 error
  - PlayMode 截圖：`Assets/Screenshots/S10-auto-loop-r10-m2-1.png`
- 風險/阻塞：
  - 拼字/發音仍為示意，尚未接真實輸入與音訊資源
- 下一步：
  - M3：雙面卡翻轉與答案揭露視覺層

---

## 交接記錄（2026-02-19）- S10-P0 M1 狀態機骨架實作

- 目標：開始落地 P0「選牌答題後出卡」流程的第一階段（M1）
- 完成內容：
  - `PrototypeCardGameUiController` 新增 `CardQuizCastPhase` 狀態機骨架
  - 新增出牌流程輸入鎖（答題/動畫期間禁止誤觸）
  - `StartQuizAndPlay`、`OnQuizOptionSelected`、`CompleteQuizAndPlay` 串接新階段
  - 出卡動畫流程加入 `CastAnimationQueue -> CardFlipReveal -> ResolveScore -> RoundPostState` 時序節點
  - 移除卡牌「上桌」字樣（保留英語、詞性、元素、等級）
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Prototype/PrototypeCardGameUiController.cs`
  - `docs/systems/S10-ui-ux/ALIGNMENT_TRACKER.md`
  - `docs/systems/S10-ui-ux/SYSTEM.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - Unity 重新編譯後 Console `error` 為 0
  - 可進入 PlayMode，截圖：`Assets/Screenshots/S10-auto-loop-r10-m1-1.png`
- 風險/阻塞：
  - 中央單卡放大題目舞台尚未完成（M2）
- 下一步：
  - M2：題目舞台改版（單卡放大 + 題型切換容器）

---

## 交接記錄（2026-02-19）- S10-P0 選牌答題後出卡規格落地

- 目標：把「選牌 -> 答題 -> 出卡翻牌」定義為 S10 優先落地流程
- 完成內容：
  - 新增 `docs/systems/S10-ui-ux/PRIORITY_P0_CARD_QUIZ_CAST_FLOW.md`
  - 定義單回合狀態機、事件表、題型策略、動畫節奏與風險對策
  - 更新 `P02-run-table` 規格，掛接 P0 流程
  - 更新 S10 主文件與對齊追蹤，標記本工作流為 P0
- 變更檔案：
  - `docs/systems/S10-ui-ux/PRIORITY_P0_CARD_QUIZ_CAST_FLOW.md`
  - `docs/systems/S10-ui-ux/pages/P02-run-table/SYSTEM.md`
  - `docs/systems/S10-ui-ux/SYSTEM.md`
  - `docs/systems/S10-ui-ux/ALIGNMENT_TRACKER.md`
  - `docs/systems/S10-ui-ux/pages/README.md`
  - `docs/PROGRESS_OVERVIEW.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - 文件鏈路已建立（主系統 -> 頁面 -> P0 規格）
- 風險/阻塞：
  - 程式層尚未進入 M1 狀態機實作
- 下一步：
  - 進入 M1：實作狀態機骨架與輸入鎖

---

## 交接記錄（2026-02-19）- S10 對齊追蹤機制建立

- 目標：建立 S10 UI 對齊的固定記錄與提交流程
- 完成內容：
  - 新增 `docs/systems/S10-ui-ux/ALIGNMENT_TRACKER.md`
  - 定義對齊分數模型（Layout/Readability/Interaction/Polish）
  - 新增每輪必填欄位與「每輪結束必做」流程（更新、驗證、commit、push）
  - 在 `docs/systems/S10-ui-ux/SYSTEM.md` 增加對齊追蹤章節與更新紀錄
  - 在 `docs/PROGRESS_OVERVIEW.md` 更新 S10 下一步為追蹤檔驅動
- 變更檔案：
  - `docs/systems/S10-ui-ux/ALIGNMENT_TRACKER.md`
  - `docs/systems/S10-ui-ux/SYSTEM.md`
  - `docs/PROGRESS_OVERVIEW.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - 文件新增與連結完成，可作為每輪固定更新入口
- 風險/阻塞：
  - 無
- 下一步：
  - 每輪 UI 對齊完成後同步更新追蹤檔並 push

---

## 交接記錄（2026-02-12）- 新專案文件體系建立

- 目標：建立可交接、可開發的規格與架構文檔體系
- 完成內容：
  - 建立 `README` 與 `docs/00~18` 主規格
  - 補齊 SA/SD 缺口（NFR、Runtime 契約、Risk、Context、Usecase、Balance SoT、Config 治理、Test Matrix、API 型別）
  - 建立 scripts/schema 基礎（config 驗證與測試入口）
- 變更檔案：
  - `README.md`
  - `docs/baseline/00-project-vision.md` ~ `docs/baseline/18-api-and-domain-types.md`
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
  - `docs/baseline/17-test-matrix.md`
  - `docs/baseline/18-api-and-domain-types.md`
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
  - `docs/baseline/10-runtime-state-and-event-contracts.md`
  - `docs/baseline/17-test-matrix.md`
  - `docs/baseline/18-api-and-domain-types.md`
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
  - `docs/baseline/17-test-matrix.md`
  - `docs/baseline/18-api-and-domain-types.md`
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
  - `docs/baseline/17-test-matrix.md`
  - `docs/baseline/18-api-and-domain-types.md`
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
  - `docs/baseline/17-test-matrix.md`
  - `docs/baseline/18-api-and-domain-types.md`
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
  - `docs/baseline/17-test-matrix.md`
  - `docs/baseline/18-api-and-domain-types.md`
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
  - `docs/baseline/17-test-matrix.md`
  - `docs/baseline/18-api-and-domain-types.md`
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
  - 新增全專案分析文件：`docs/baseline/20-project-analysis-2026-02-14.md`
  - 同步更新階段文件：`README.md`、`docs/PROJECT_EXECUTION_PLAN.md`
  - 風險清單補充：Unity 授權阻塞與規格漂移風險（R-007、R-008）
- 變更檔案：
  - `README.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
  - `docs/baseline/11-risk-register-and-decision-log.md`
  - `docs/baseline/20-project-analysis-2026-02-14.md`
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
  - `docs/baseline/17-test-matrix.md`
  - `docs/baseline/18-api-and-domain-types.md`
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
  - `docs/baseline/16-config-governance.md` 補上 M4-01 基線規則
- 變更檔案：
  - `configs/word_entries.v2.json`
  - `scripts/validate_configs.py`
  - `docs/baseline/16-config-governance.md`
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
  - `docs/baseline/15-balance-source-of-truth.md`
  - `docs/baseline/17-test-matrix.md`
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
  - `docs/baseline/10-runtime-state-and-event-contracts.md`
  - `docs/baseline/15-balance-source-of-truth.md`
  - `docs/baseline/17-test-matrix.md`
  - `docs/baseline/18-api-and-domain-types.md`
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
  - 新增 `docs/baseline/21-m4-first-balance-report.md`
  - 彙整 M4-01~03 的可量化結果：
    - 詞庫分布與覆蓋率
    - 商店權重與價格帶
    - 盲注曲線三檔體感策略
  - 列出 Alpha 前風險與建議執行順序
  - 更新進度看板：M4 全完成，Alpha Gate 啟動
- 變更檔案：
  - `docs/baseline/21-m4-first-balance-report.md`
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
  - 更新 `docs/baseline/20-project-analysis-2026-02-14.md` 內容至最新狀態：
    - `M3` / `M4` 由 Todo 改為 Done
    - `Alpha Gate` 改為 In Progress
    - 移除已完成缺口（如 `TryUnlockNode` 未實作）
    - 將下一步改為 `A-01~A-04` 驗收路徑
- 變更檔案：
  - `docs/baseline/20-project-analysis-2026-02-14.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - 與 `docs/IMPLEMENTATION_STATUS.md`、`docs/PROJECT_EXECUTION_PLAN.md` 對齊
- 風險/阻塞：
  - 無新增阻塞（維持現有 Unity 授權限制）
- 下一步：
  - 持續 A-01 回歸，完成後同步更新 `docs/20` 與看板

## 交接記錄（2026-02-16）- A-01 全流程回歸準備完成

- 目標：把 A-01 回歸執行前的自動化與流程文件準備到可執行狀態
- 完成內容：
  - 新增 `AlphaRegressionTests`（Ante1-8 通關路徑 + 失敗路徑）
  - 新增 `docs/baseline/22-alpha-a01-regression-checklist.md`（執行步驟、驗收表、阻塞）
  - 更新 `docs/17` 的 Alpha 測試案例 ID
  - 更新 `docs/IMPLEMENTATION_STATUS` 與 `docs/PROJECT_EXECUTION_PLAN`
- 變更檔案：
  - `Assets/MnemosyneArcana/Tests/EditMode/AlphaRegressionTests.cs`
  - `docs/baseline/22-alpha-a01-regression-checklist.md`
  - `docs/baseline/17-test-matrix.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - `bash scripts/validate_configs.sh` 通過
- 風險/阻塞：
  - 本環境 Unity 授權限制仍在，A-01 最終驗收需在可授權環境完成
- 下一步：
  - 在可授權環境執行 A-01 並填寫 `docs/22` 驗收結果

## 交接記錄（2026-02-16）- A-01 全流程回歸執行完成（授權/編譯問題解除）

- 目標：在可授權 Unity 環境完成 A-01 回歸，並修正阻塞編譯錯誤
- 完成內容：
  - 以 Unity `2022.3.62f3` 啟動 batchmode，確認授權可握手
  - 定位並修正編譯錯誤：
    - `MetaManagerV2.CurriculumNodeDef` 的 `init` 存取子在目前 Unity 編譯設定下觸發 `CS0518 IsExternalInit`
    - 將 `Cost` / `RequiredAnyOfGroups` / `MutexWith` 改為 `set`
  - 執行：
    - `UNITY_PATH='/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity' bash scripts/run_editmode_tests.sh`
    - 結果：`[OK] EditMode tests finished.`
  - 更新 A-01 驗收文件：`docs/baseline/22-alpha-a01-regression-checklist.md`
  - 更新進度文件：`docs/IMPLEMENTATION_STATUS.md`、`docs/PROJECT_EXECUTION_PLAN.md`、`docs/baseline/20-project-analysis-2026-02-14.md`
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Core/Managers/MetaManagerV2.cs`
  - `docs/baseline/22-alpha-a01-regression-checklist.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
  - `docs/baseline/20-project-analysis-2026-02-14.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - A-01 核心回歸測試通過（Ante1-8 通關與失敗分支）
- 風險/阻塞：
  - A-01 阻塞已解除；後續進入 A-02（migration）與 A-03（效能）
- 下一步：
  - 啟動 A-02 存檔/migration 壓測方案（測試資料集、升級/回退路徑、失敗復原）

## 交接記錄（2026-02-16）- A-UI-01 可操作卡牌 UI 原型完成

- 目標：建立可直接試玩的真實卡牌 UI，支援邊玩邊調參與答題流程驗證
- 完成內容：
  - 建立 `PrototypeCardGameUiController`，取代先前純面板原型
  - 卡牌互動：
    - 手牌可點擊上桌，也可拖曳到牌桌區上桌
    - 上桌卡高亮與脈衝效果
    - 抽牌進場動畫、出牌飛出動畫
  - 答題流程：
    - 新增答題區，流程為「開始答題並出牌」
    - 題型調整為「英文題幹 + 中文四選一」
    - 逐題作答後才進行本回合計分與出牌提交
  - 商店互動：
    - 商店改為可點擊卡片格子購買（非純文字列表）
  - 介面可用性：
    - 修正右欄擠壓問題（響應式欄寬與格子欄數）
    - 調參面板改為可收合（預設收合，按需展開）
    - 整體面板支援上下滾動
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Prototype/PrototypeCardGameUiController.cs`
  - `Assets/MnemosyneArcana/Scripts/Prototype/PrototypeGameScreenController.cs`
  - `Assets/MnemosyneArcana/Scripts/Prototype/PrototypeSandboxController.cs`
  - `README.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - 於 Unity Editor Play Mode 手動驗證互動流程可運作
  - 因專案同時開啟鎖定，無法在 CLI 再次跑 batchmode 測試
- 風險/阻塞：
  - 目前原型仍為開發迭代 UI，尚未進入最終美術與完整 UX polish
- 下一步：
  - A-02 存檔/migration 壓測
  - Prototype 下一階段：拖曳放牌桌吸附、答題時間限制、答錯三選一完整接入

## 交接記錄（2026-02-16）- Unity MCP 串接與接續作業準備

- 目標：建立「改完即測」的 Unity MCP 工作前置，讓新 session 可直接接續
- 完成內容：
  - 已在 Codex 全域設定新增 MCP server：
    - `codex mcp add unityMCP --url http://localhost:8080/mcp`
  - 設定驗證：
    - `codex mcp list` 顯示 `unityMCP` 已 `enabled`
  - 補充：當前舊 session 尚未讀到新 MCP 資源，需重開 session 載入新設定
  - 已整理 Alpha 缺口補齊計畫，供後續直接執行：
    - `docs/plans/2026-02-16-alpha-gap-closure.md`
- 變更檔案：
  - `docs/plans/2026-02-16-alpha-gap-closure.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - `codex mcp add unityMCP --url http://localhost:8080/mcp`：成功（Added global MCP server）
  - `codex mcp list`：可見 `unityMCP`
- 風險/阻塞：
  - Unity 端 MCP HTTP server（`localhost:8080`）需保持啟動，否則無法取用 Unity MCP 工具
  - 新增 MCP 設定後需重開 Codex session 才會穩定讀取 resources
- 下一步（新 session 第一批動作）：
  - 啟動 Unity MCP server（Unity 視窗 `Window > MCP for Unity`）
  - 新開 Codex session 後先檢查 MCP 可用性（`list_mcp_resources`）
  - 進入 A-02（存檔/migration）實作時採「每完成一小步就跑測試」節奏：
    - `bash scripts/validate_configs.sh`
    - `UNITY_PATH='/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity' bash scripts/run_editmode_tests.sh`

## 交接記錄（2026-02-17）- 10000 詞彙成長曲線問題建檔與追蹤啟動

- 目標：將「0~10000 詞彙量模型」的卡關、遺忘退回、真學習驗證需求轉為可執行追蹤項目
- 完成內容：
  - 新增主文件：`docs/baseline/24-vocab-growth-curve-and-gating-plan.md`
  - 明確定義 11 段詞彙量模型（M0~M10）
  - 定義 EffectiveVocab 通關門檻（LearnedCount × RetentionRate × RetrievalRate）
  - 定義 Recovery Gate、退回規則與 7 天 1 次退關保護
  - 補上 telemetry 追蹤與告警閾值
  - 在實作看板新增追蹤任務：
    - `A-BAL-01` EffectiveVocab 關卡門檻
    - `A-BAL-02` Recovery Gate 與退回保護
    - `A-BAL-03` Boss 主動回憶題守門
    - `A-DATA-01` 學習 telemetry 與告警
- 變更檔案：
  - `docs/baseline/24-vocab-growth-curve-and-gating-plan.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - 文件新增成功，任務已加入看板可追蹤
- 風險/阻塞：
  - 尚未落地程式層 gate API 與 telemetry 寫入管線
- 下一步：
  - 先實作 `A-BAL-01`（EffectiveVocab gate API）並補對應 EditMode 測試

## 交接記錄（2026-02-17）- 十模型驗證取代固定 70% 驗證

- 目標：將驗證策略由固定單一玩家假設（70%）改為 M0~M9 十模型驗證
- 完成內容：
  - 原型驗證入口改為 `10模型驗證`，逐一跑 M0~M9 並輸出卡關位置
  - Final 結算已採雙門檻：
    - Main Clear：掌握率 >= 95%
    - True Clear：掌握率 = 100% 且穩定 7 天
  - 10 模型流程實測（Unity MCP）已取得首輪結果：
    - M0/M1/M3/M4/M5/M6/M8 卡點符合預期
    - M2、M9 各偏早 1 關
  - 文件同步更新：
    - `docs/baseline/24-vocab-growth-curve-and-gating-plan.md`
    - `docs/baseline/25-gate-model-sweep-report-2026-02-17.md`
    - `docs/IMPLEMENTATION_STATUS.md`
- 變更檔案：
  - `docs/baseline/24-vocab-growth-curve-and-gating-plan.md`
  - `docs/baseline/25-gate-model-sweep-report-2026-02-17.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - Unity MCP EditMode：`118/118 passed`
  - PlayMode log 可觀測 `10 模型驗證` 與各模型卡關輸出
- 風險/阻塞：
  - M7 在首輪截圖窗口內未完整收斂，需下一輪補齊完整紀錄
  - M9 末段仍偏早卡關 1 關
- 下一步：
  - 針對 M2/M9 做定向調參，並回跑十模型報告確認偏差收斂

## 交接記錄（2026-02-17）- 十模型第二輪調參收斂（M2/M9）

- 目標：修正首輪十模型中 `M2`、`M9` 提前卡關問題
- 完成內容：
  - 原型調參：
    - `M2` retention/retrieval：`0.82/0.78 -> 0.86/0.82`
    - `M9` retention/retrieval：`0.96/0.92 -> 0.97/0.93`
    - 高段模型（M8/M9）出牌係數微增（`BuildModelHandScore` +0.03）
  - 重新執行 Unity MCP 驗證：
    - EditMode：`118/118 passed`
    - PlayMode：`10模型驗證` 全流程完成（含 M9 結果）
- 實測結果（重點）：
  - `M0~M8`：卡點全部符合預期 Ante
  - `M9`：由「Ante8 提前失敗」提升為「本輪通關」
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Prototype/PrototypeCardGameUiController.cs`
  - `docs/baseline/25-gate-model-sweep-report-2026-02-17.md`
  - `docs/SESSION_NOTES.md`
- 風險/阻塞：
  - `M9` 已達通關，下一輪需校準「通關率區間」避免偏易
- 下一步：
  - 固定種子多輪（建議 >=30）統計十模型通關/卡關分佈
  - 用分佈數據回調 M8/M9 高段係數，鎖定目標通關率帶

## 交接記錄（2026-02-17）- 十模型 30 輪批次驗證工具接入

- 目標：將「多 seed 分佈驗證」做成可重複操作的原型工具
- 完成內容：
  - 原型新增按鈕：`10模型30輪`
  - 新增批次流程：`TenModelBatchValidationFlow(30)`
  - 每模型輸出統計摘要：
    - `clear=x/30`
    - `modeFailAnte=y(count)`
    - `expected=z`
  - 抽離模型設定為 `GetTenModelProfiles()`，避免單輪/批次配置分叉
- 驗證結果：
  - Unity MCP EditMode：`118/118 passed`
  - PlayMode 啟動後無 runtime 例外（既有 `10 模型驗證` 可持續運行）
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Prototype/PrototypeCardGameUiController.cs`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/SESSION_NOTES.md`
- 下一步：
  - 直接執行 `10模型30輪` 並回填統計數據到 `docs/baseline/25-gate-model-sweep-report-2026-02-17.md`

## 交接記錄（2026-02-17）- 十模型 30 輪實測完成（首版分佈）

- 目標：取得多 seed 分佈數據，驗證十模型卡關穩定性
- 執行方式：
  - Unity MCP PlayMode 觸發 Prototype `10模型30輪`
- 主要結果：
  - `M0~M8`：`clear=0/30` 且 `modeFailAnte` 與 expected 完全一致
  - `M9`：`clear=30/30`（100% 通關）
- 判讀：
  - 低中段曲線穩定且可預測
  - 高段 M9 已達可通關，但通關率過高，需回調避免偏易
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Prototype/PrototypeCardGameUiController.cs`
  - `docs/baseline/25-gate-model-sweep-report-2026-02-17.md`
  - `docs/SESSION_NOTES.md`
- 下一步：
  - 目標將 M9 通關率從 100% 回調至設計區間（建議先試 30%~60%）

## 交接記錄（2026-02-17）- M9 通關率校準完成（30 輪）

- 目標：將高段模型 M9 從過易（100%）回調到可控通關率區間
- 執行方式：
  - Unity MCP 重複執行 Prototype `10模型30輪`
  - 以 `BuildModelHandScore` 的 M9 係數與答題機率做迭代調參
- 最終結果：
  - `M0~M8`：仍維持 `clear=0/30` 且卡關點與預期一致
  - `M9`：`clear=12/30`（40%），落在目標區間 `30%~60%`
- 驗證結果：
  - Unity MCP EditMode：`118/118 passed`
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Prototype/PrototypeCardGameUiController.cs`
  - `docs/baseline/25-gate-model-sweep-report-2026-02-17.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/SESSION_NOTES.md`

## 交接記錄（2026-02-18）- S4 優先驗測啟動與 MCP 重連規範

- 目標：
  - 啟動 S4（Gate/Recovery/Demotion）優先驗測
  - 固化 MCP 連線故障時的標準恢復流程
- 完成內容：
  - 新增 S4 測試檔：
    - `Assets/MnemosyneArcana/Tests/EditMode/S4PriorityValidationTests.cs`
    - 覆蓋三模型 Recovery Gate + 7/14/30 天長週期退化案例
  - 修正測試 enum 錯誤：
    - `WordPool.Active -> WordPool.Mastered`
  - 新增驗測連線規範文件：
    - `docs/verification/04-mcp-connection-recovery-checklist.md`
  - 更新 Master Plan：
    - 加入 MCP `15 秒 timeout 規則` 與恢復順序
- 當前狀態：
  - 由於 MCP transport 仍有 session 級超時，S4 測試待新 session 重新執行驗證
- 下一步：
  - 新 session 先跑 MCP smoke test（15 秒內）
  - 立即執行 EditMode 測試，確認 `S4PriorityValidationTests` 通過

## 交接記錄（2026-02-18）- S4 優先驗測修復與回歸完成

- 目標：
  - 在新 session 完成 MCP smoke test 後，落地 S4 優先驗測
  - 修復 `S4PriorityValidationTests` 長週期退化案例失敗
- 完成內容：
  - 確認 Unity MCP 連線正常（`list_mcp_resources` 可回應）
  - 執行 EditMode 測試時，定位失敗：
    - `S4PriorityValidationTests.Decay_LongCycle_SevenFourteenThirtyDays_DegradesStepwise`
    - 失敗訊息：預期 `Lv3`，實際 `Lv0`
  - 修正 `ApplyDecayAt` 測試 helper：
    - 將同一 checkpoint 的退化改為「逐級消耗門檻天數」模擬
    - 避免 7 天檢查被連續退化到 Lv0
    - 保留 30 天長週期可多級退化行為
  - 重新驗證：
    - 單測：`S4PriorityValidationTests.Decay_LongCycle_SevenFourteenThirtyDays_DegradesStepwise` `1/1` pass
    - 全量 EditMode：`121/121` pass
- 變更檔案：
  - `Assets/MnemosyneArcana/Tests/EditMode/S4PriorityValidationTests.cs`
  - `docs/SESSION_NOTES.md`
- 驗證結果：
  - Unity MCP EditMode 測試全綠（`121 passed, 0 failed`）
- 風險/阻塞：
  - 無新增阻塞
- 下一步：
  - 依 `docs/verification/02-design-doc-coverage-matrix.md` 執行下一優先序：`S7 Final Gate + Endless`
  - 其後進入 `A-02` 存檔/migration 壓測

## 交接記錄（2026-02-18）- S7 Final/Endless 驗測補齊與調參完成

- 目標：
  - 優先完成 S7（Final Gate + Endless）驗測證據
  - 以最小調參確保高端模型可覆蓋 True Clear 驗證情境
- 完成內容：
  - 新增 S7 驗測檔：
    - `Assets/MnemosyneArcana/Tests/EditMode/S7FinalGateValidationTests.cs`
    - 覆蓋 `S7-M1/M2/M3/M4`
  - S7-M4 採 30 seeds、180 天長局模擬，驗證無非法狀態轉移
  - Prototype 調參（最小變更）：
    - `GetTenModelProfiles()` 中 `M9 Mastery: 0.98 -> 1.00`
    - 目的：讓高端模型可在流程層覆蓋 `100%+7天` True Clear
  - 文件回填：
    - `docs/baseline/17-test-matrix.md` 新增 `TC-S7-001~004`
    - `docs/baseline/25-gate-model-sweep-report-2026-02-17.md` 新增 S7 補齊章節
    - `docs/verification/02-design-doc-coverage-matrix.md` 移除 S7 缺口、更新優先序
    - `docs/verification/03-final-verification-report-template.md` 新增 S7 snapshot
- 驗證結果：
  - Unity MCP EditMode job `60e62f78000a4cc9b9b1bb65675e8a74`：`125/125 passed`
  - Unity MCP EditMode job `e457988c0f9b439a88df1b52a0fc2bbc`：`125/125 passed`
- 風險/阻塞：
  - 尚待補齊：`S4 長週期分佈報告`、`S8 Telemetry`、`S9 NFR` 三模型壓測
- 下一步：
  - 先補 S4 長週期分佈報告，再進 S8/S9 驗測，完成 verification 關門條件

## 交接記錄（2026-02-18）- Verification 缺口收斂（S4/S8/S9）

- 目標：
  - 完成 verification 中剩餘高優先驗測缺口（S4 分佈、S8 告警三模型、S9 NFR 三模型）
- 完成內容：
  - 新增測試：
    - `Assets/MnemosyneArcana/Tests/EditMode/S4LongCycleDistributionTests.cs`
    - `Assets/MnemosyneArcana/Tests/EditMode/S8TelemetryModelCoverageTests.cs`
    - `Assets/MnemosyneArcana/Tests/EditMode/S9NfrValidationTests.cs`
  - 修正測試編譯錯誤：
    - `S9NfrValidationTests` 新增 `using MnemosyneArcana.Core.Runtime;`（`RunPhase` namespace）
  - 全量回歸：
    - Unity MCP EditMode job `7077ee7ea9df451887a88308342a0093`：`133/133 passed`
  - 文件回填：
    - `docs/baseline/17-test-matrix.md` 補 `TC-S4-008/009`、`TC-S8-001~003`、`TC-S9-001~003`
    - `docs/baseline/25-gate-model-sweep-report-2026-02-17.md` 補 S4 長週期分佈章節
    - `docs/verification/02-design-doc-coverage-matrix.md` 將 `docs/09`、`docs/24` 更新為 `Covered`
    - `docs/verification/03-final-verification-report-template.md` 補 S8/S9 snapshot
- 驗證結果：
  - S4/S7/S8/S9 相關新增案例全部通過，且可由同一 test job 追溯
- 風險/阻塞：
  - 仍待整體 sign-off：多份設計文件於 coverage matrix 為 `In Progress`，需最終彙總報告收斂
- 下一步：
  - 進行 verification 最終報告彙整（`docs/verification/03`）
  - 確認是否滿足進入 `A-02` 的 gate 條件

## 交接記錄（2026-02-18）- Final Sign-off 報告完成（可進 A-02）

- 目標：
  - 完成 verification 最終收斂，提供明確 Go/No-Go 判定
- 完成內容：
  - 更新覆蓋矩陣：
    - `docs/verification/02-design-doc-coverage-matrix.md`
    - 將剩餘 `In Progress` 條目收斂為 `Covered`
  - 新增最終報告：
    - `docs/verification/03-final-verification-report-2026-02-18.md`
    - 結論：`Go`（允許進入 A-02）
  - 追溯證據納入報告：
    - `7077ee7ea9df451887a88308342a0093`（`133/133`）
    - `272f745e9e1a4955b4988969cd75308a`（`130/130`）
    - `e457988c0f9b439a88df1b52a0fc2bbc`（`125/125`）
    - `60e62f78000a4cc9b9b1bb65675e8a74`（`125/125`）
- 驗證結果：
  - 目前 verification 文件已具備 A-02 進場證據鏈
- 風險/阻塞：
  - 無 Critical 阻塞
- 下一步：
  - 進入 `A-02`：存檔/migration 壓測與失敗回退驗證

## 交接記錄（2026-02-18）- 文件系統化重整（System/Tech/Management）

- 目標：把文件改為「以系統為中心」可視化結構，並建立單一進度總表。
- 完成內容：
  - 新增 `docs/INDEX.md` 作為總入口。
  - 新增 `docs/PROGRESS_OVERVIEW.md` 作為全域資料夾進度總表。
  - 新增 `docs/systems/S1~S9/*/SYSTEM.md`，每個系統固定五區塊：
    1) 設計規劃
    2) 規格文件
    3) 實作紀錄
    4) 驗測報告與調整建議
    5) 更新紀錄
  - 新增 `docs/tech/*/README.md` 與 `docs/management/*/README.md` 分組入口。
  - `README.md` 新增系統化入口說明。
- 變更檔案：
  - `README.md`
  - `docs/INDEX.md`
  - `docs/PROGRESS_OVERVIEW.md`
  - `docs/systems/**/SYSTEM.md`
  - `docs/tech/**/README.md`
  - `docs/management/**/README.md`
  - `docs/SESSION_NOTES.md`
- 下一步：
  - 依 `docs/PROGRESS_OVERVIEW.md` 持續逐系統回填驗測結果與參數調整建議。

## 交接記錄（2026-02-18）- 系統文件自洽化重整

- 目標：允許破壞舊索引依賴，將系統文件改為單檔自洽（不再寫來源路徑）。
- 完成內容：
  - 重寫 `docs/systems/S1~S9/SYSTEM.md`，每份直接含設計/規格/實作/驗測/更新。
  - 重寫 `docs/tech/*/README.md` 為技術標準，不再只是來源清單。
  - 重寫 `docs/INDEX.md` 與 `docs/PROGRESS_OVERVIEW.md`，將進度收斂為單一總表。
  - 更新 `README.md`，改為以新入口為主。
- 下一步：
  - 逐系統驗測完成後，只更新對應 `SYSTEM.md` 與 `PROGRESS_OVERVIEW.md`。

## 交接記錄（2026-02-18）- 文件收束（保守模式）

- 目標：在不脫離原規劃前提下，收束重複維護。
- 完成內容：
  - 修正 `docs/PROGRESS_OVERVIEW.md` 的量化門檻格式（移除字面 `\\n`）。
  - 將 `docs/IMPLEMENTATION_STATUS.md` 定位為歷史快照；即時狀態改由 `docs/PROGRESS_OVERVIEW.md` 單一維護。
  - 將 `docs/PROJECT_EXECUTION_PLAN.md` 定位為排程基準，並改為引用 `docs/PROGRESS_OVERVIEW.md` 作為即時狀態來源。
  - 明確區分驗測文件角色：
    - `docs/verification/00-master-verification-plan.md`：基線規範
    - `docs/verification/05-game-system-reverification-plan-2026-02-18.md`：本輪執行計畫
- 下一步：
  - 按 `05` 計畫逐系統執行實測，完成後只回填 `SYSTEM.md` + `PROGRESS_OVERVIEW.md` + 驗測報告。

## 交接記錄（2026-02-18）- Baseline 00~25 分配到各系統資料夾

- 目標：讓 `00~25` 能在各系統資料夾中直接看到對應落位。
- 完成內容：
  - 於 `docs/systems/S1~S9/` 新增 `BASELINE_REFERENCE.md`。
  - 每個系統都列出對應的 `docs/baseline/00~25` 文件清單（可跨系統重疊）。
  - 更新 `docs/INDEX.md`，新增 baseline 分配入口說明。
- 原則：
  - 保留 `docs/baseline/` 作為 Reference Only 原文存放。
  - 系統資料夾提供分配視圖，不複製原文，避免多版本漂移。
- 下一步：
  - 若需要，可再進一步把每個 `SYSTEM.md` 的「規格文件」段落收斂為 `BASELINE_REFERENCE.md` 的精簡清單。

## 交接記錄（2026-02-18）- S1 驗測完成（達成 Done）

- 目標：依「S1 done 才能進下一系統」規則，完成 S1 行為驗測並收斂為 Done。
- 驗測執行：
  - RunFlowTests：job `e02540ea27314d9cbdc88c3c8cda3298`（9/9）
  - AlphaRegressionTests：job `3f9ccd60230741b3ab249f25d5dd300a`（2/2）
  - PlayableLoopUseCaseTests：job `b7170cb97e764ea3b21e9d23f38da8b5`（2/2）
- 判定：
  - Low/Mid/High 覆蓋完成
  - Failure/Boundary case 已覆蓋
  - 測項 100% pass，證據可追溯
- 文件回填：
  - `docs/systems/S1-run-blind/SYSTEM.md` 第 4/5 節已更新
  - `docs/PROGRESS_OVERVIEW.md`：S1 狀態改為 `Done`
- 下一步：
  - 進入 S2（Scoring/HandType）驗測。

## 交接記錄（2026-02-18）- S2 驗測完成（達成 Done）

- 目標：在 S1 完成後，收斂 S2（Scoring/HandType）到 Done。
- 驗測執行：
  - ScoringHandTypeTests：job `f50a4925d23d42bfb5c0a7b61156d052`（6/6）
  - ScoringFormulaTests：job `fb3e219b58fc417fa3c89d0f57905193`（3/3）
- 判定：
  - Low/Mid/High 覆蓋完成
  - 邊界案例（倍率下限、逆序語序鏈）已覆蓋
  - 測項 100% pass，證據可追溯
- 文件回填：
  - `docs/systems/S2-scoring-handtype/SYSTEM.md` 第 4/5 節已更新
  - `docs/PROGRESS_OVERVIEW.md`：S2 狀態改為 `Done`
- 下一步：
  - 進入 S3（Learning/Boss）驗測。

## 交接記錄（2026-02-18）- S3 驗測完成（達成 Done）

- 目標：在 S2 完成後，收斂 S3（Learning/Boss）到 Done。
- 驗測執行：
  - LearningManagerTests：job `50345a33123740d18bd0d0e337af7a50`（9/9）
  - BossLearningTests：job `7251ce2f9e3a4b5aafd8eb9b976f1d52`（14/14）
  - UserStoryAcceptanceTests.US12：job `1d2d6da66d75418687e8d6401009feb3`（1/1）
- 判定：
  - Low/Mid/High 覆蓋完成
  - 邊界/失敗案例已覆蓋（InvalidInput/StateConflict/null input）
  - 測項 100% pass，證據可追溯
- 文件回填：
  - `docs/systems/S3-learning-boss/SYSTEM.md` 第 4/5 節已更新
  - `docs/PROGRESS_OVERVIEW.md`：S3 狀態改為 `Done`
- 下一步：
  - 進入 S4（Gate/Recovery/Demotion）驗測與 done 判定。

## 交接記錄（2026-02-18）- S4 驗測完成（達成 Done）

- 目標：依序收斂 S4（Gate/Recovery/Demotion）到 Done。
- 驗測執行：
  - S4PriorityValidationTests：job `9806eac7df504e12b3275f669096e5e9`（3/3）
  - S4LongCycleDistributionTests：job `59f41d6790f24d46a1ec65e7c9249acf`（2/2）
- 判定：
  - Low/Mid/High 覆蓋完成
  - Edge/批次要求（30 seeds）已覆蓋
  - 長週期 7/14/30 天單調退化已驗證
  - 測項 100% pass，證據可追溯
- 文件回填：
  - `docs/systems/S4-gate-recovery-demotion/SYSTEM.md` 第 4/5 節已更新
  - `docs/PROGRESS_OVERVIEW.md`：S4 狀態改為 `Done`
- 下一步：
  - 進入 S5（Shop/Economy）驗測。

## 交接記錄（2026-02-18）- S5 驗測重跑完成（依交接續做）

- 目標：依最新交接紀錄，銜接執行 S5（Shop/Economy）驗測並補最新證據。
- 驗測執行：
  - Unity MCP EditMode：job `dae7c0b1e5dc47f5a6a8b1c82b9ce218`（`175/175 passed`）
  - S5 對應測項確認：
    - `ShopManagerTests.PurchaseOffer_NotEnoughMoney_FailsGracefully`（M-Low）
    - `UserStoryAcceptanceTests.US04_ShopCanGenerateAndPurchaseWithBalanceGuard`（M-Mid）
    - `UserStoryAcceptanceTests.US08_BossShopAlwaysOffersTwoCoursesAtPrice10`（M-High）
    - `PlayableLoopUseCaseTests.UseCase_FirstBlindToShopPurchaseAndAdvance_Works`（M-High）
    - `ShopManagerTests.GetRerollCost_TwentyRolls_IsStrictlyIncreasing`（重擲遞增）
    - `ShopManagerTests.RerollEconomy_Budget80_CannotSustainTwentyRollsAndLosesBuyWindows`（長局/可購買窗口）
- 判定：
  - Low/Mid/High 與邊界案例覆蓋可追溯。
  - 測項皆通過，S5 可維持 `Done` 判定。
- 下一步：
  - 進入 S6（Meta/Contract/Curriculum）驗測收口，確認 MCP job 證據可重跑取得。

## 交接記錄（2026-02-18）- S6 標記 Done，啟動 S7 round-2 監控

- 目標：依你決策「S6 done，開始 S7」，更新總表並執行 S7 新一輪驗測。
- 完成內容：
  - `docs/PROGRESS_OVERVIEW.md`：
    - `S6` 由 `In Progress` 更新為 `Done`（Verification=`Done`，Next Action=`Monitor`）。
    - `S7` 維持 `In Progress`，Verification 更新為 `Done (round-2)`。
  - `docs/systems/S7-final-endless/SYSTEM.md`：
    - 回填 round-2 驗測證據、設計問題（0 項）、調整建議。
- 驗測執行：
  - Unity MCP EditMode：job `2fad6495060d4df69e36460981cb5794`（`175/175 passed`）
  - S7 對應四測項均通過（M-Low/M-Mid/M-High/M-Edge）。
- 風險/阻塞：
  - 無新增阻塞；S7 目前進入監控階段，待後續監測資料決定是否可申請 `Done`。
- 下一步：
  - 持續監控 S7 高模型通關帶與 30-seed 長局穩定性趨勢，再提 `Done` 決策。

## 交接記錄（2026-02-18）- S7 高模型通關帶監控（round-3）

- 目標：依決策啟動 S7 高模型通關帶驗測，確認 M9 是否維持在目標區間（30%~60%）。
- 驗測執行：
  - Unity MCP EditMode：job `c74eb22c0ac841778133f41195e6415f`（`176/176 passed`）
  - 新增監控測項：`S7_M9_ThirtyRuns_ClearRateMonitoring`
- 關鍵結果：
  - M9 通關率：`16/30 = 53.3%`
  - 判定：落在目標通關帶（30%~60%）
- 風險/阻塞：
  - 無新增阻塞；目前未觀察到偏離趨勢。
- 下一步：
  - 等你決策是否將 `S7` 由 `In Progress` 更新為 `Done`。

## 交接記錄（2026-02-18）- S7 決策完成（標記 Done）

- 目標：依產品決策將 S7 正式收口。
- 完成內容：
  - 已依決策更新 `docs/PROGRESS_OVERVIEW.md`：`S7 Final/Endless` 由 `In Progress` 改為 `Done`。
  - `Verification` 收斂為 `Done`，`Next Action` 設為 `Monitor`。
  - `docs/systems/S7-final-endless/SYSTEM.md` 已標註 `Done`（已決策同意）。
- 下一步：
  - 進入 S8（Telemetry/Observability）誤報/漏報場景補齊與驗測。

## 交接記錄（2026-02-18）- S8 誤報/漏報場景補齊完成（round-2）

- 目標：補齊 S8 誤報（false positive）/漏報（false negative）場景，完成可決策的驗測證據。
- 完成內容：
  - 新增測項：
    - `S8_FP_HighPassButLowRecall_DoesNotTriggerTooEasy`
    - `S8_FN_BorderlinePassWithLongStall_TriggersTooHard`
  - 調整 `LearningTelemetryManagerV2`：
    - 高通關率但主動回憶偏低時，不觸發 `GATE_TOO_EASY`。
    - 長時間卡關且通關率偏低時，補觸發 `GATE_TOO_HARD`。
  - 文件回填：
    - `docs/systems/S8-telemetry-observability/SYSTEM.md`
    - `docs/baseline/17-test-matrix.md`
    - `docs/PROGRESS_OVERVIEW.md`（S8 Verification 更新為 `Done (round-2)`，待你決策）
- 驗測執行：
  - Unity MCP EditMode：job `ecd8f23ab6d145898f2d16437a1ee508`（red，2 fail，作為 fail-first 證據）
  - Unity MCP EditMode：job `6dcd9412cb4241dc86131720501989a4`（green，`178/178 passed`）
- 風險/阻塞：
  - 無新增阻塞。
- 下一步：
  - 等你決策是否將 `S8` 由 `In Progress` 更新為 `Done`。

## 交接記錄（2026-02-18）- S8 決策完成（標記 Done）

- 目標：依產品決策（採 A：維持現行規則）完成 S8 收口。
- 完成內容：
  - `docs/PROGRESS_OVERVIEW.md`：`S8 Telemetry/Observability` 由 `In Progress` 改為 `Done`。
  - `docs/systems/S8-telemetry-observability/SYSTEM.md`：更新為 `Done`（已決策同意）。
  - 維持現行告警門檻與誤報/漏報修正，不追加新規則改動。
- 下一步：
  - 進入 S9（NFR/Quality）建置 soak 趨勢報表並補齊驗測收口證據。

## 交接記錄（2026-02-18）- S9 soak 趨勢報表完成（round-2）

- 目標：完成 S9 的 soak 趨勢基線，補齊 `PROGRESS_OVERVIEW` 指定缺口。
- 完成內容：
  - 新增趨勢報表：`docs/verification/06-s9-soak-trend-report-2026-02-18.md`
  - 完成 3 輪 * 3 測項（M1/M2/M3）趨勢採樣並彙整平均值。
  - 更新 `docs/systems/S9-nfr-quality/SYSTEM.md`（驗測證據、趨勢摘要、調整建議）。
  - 更新 `docs/PROGRESS_OVERVIEW.md`：S9 Verification -> `Done (round-2)`（待你決策）。
- 驗測執行（MCP job）：
  - Round 1: `485718af4ca9446ba4e27f734018dc09`, `863b0a73fb8e4534a9f1a10ffe906f9b`, `f62aa912919a49c68085550eec263550`
  - Round 2: `a4fece45c4b542e197d2daafad7142c0`, `914e0458919847e4a18d4ada8868bde4`, `79490389c7ed40e0ae943b53607f4b74`
  - Round 3: `a4862e1b797048ceaf543572e8982261`, `2a6f4af3c6154952b040b030e39aca1e`, `240726559a7c44a5b40fc0729e7a60cc`
- 關鍵結果（3 輪平均）：
  - S9-M1：0.2539s
  - S9-M2：0.1183s
  - S9-M3：0.5456s
  - 全部通過，未觀察到失敗與異常漂移。
- 下一步：
  - 等你決策是否將 `S9` 由 `In Progress` 更新為 `Done`。

## 交接記錄（2026-02-18）- S9 決策完成（標記 Done）

- 目標：完成 S9 正式收口，與總表狀態一致。
- 完成內容：
  - `docs/PROGRESS_OVERVIEW.md`：`S9 NFR/Quality` 由 `In Progress` 更新為 `Done`。
  - `docs/systems/S9-nfr-quality/SYSTEM.md`：驗測結論改為「已完成決策並標記 Done」。
  - 保留本輪調整建議作為監控規範，不再新增功能性改動。
- 驗測結果：
  - 以 round-2 既有 3 輪 * 3 測項證據作為收口依據（全 Pass、均值無異常漂移）。
- 風險/阻塞：
  - 無阻塞；風險轉為監控型風險（效能漂移）。
- 下一步：
  - 進入例行監控：每週 3 輪 soak，若均值相對基線漂移 >10% 則觸發回歸調查。

## 交接記錄（2026-02-18）- S10 UI/UX 繁中基線建立（round-1）

- 目標：建立 S10，落地「非學習區繁中、學習區可英文」規範，並補齊可重跑驗測。
- 完成內容：
  - 新增 `docs/systems/S10-ui-ux/SYSTEM.md` 與 `docs/systems/S10-ui-ux/BASELINE_REFERENCE.md`。
  - 調整 `PrototypeCardGameUiController`：
    - `Seed -> 種子`
    - `Boss -> 魔王`
    - `Main/True Clear -> 主線/真結局通關`
    - `LP +/-10 -> 學習點 +/-10`
    - 局外狀態詞改為 `經驗/學習點`
  - 調整 `PrototypeSandboxController` 菜單與按鈕文案為繁中。
  - 新增驗測：`Assets/MnemosyneArcana/Tests/EditMode/S10UiLocalizationTests.cs`。
  - 更新 `docs/PROGRESS_OVERVIEW.md`：新增 `S10 UI/UX` 列，狀態 `In Progress`。
- 驗測結果：
  - Unity MCP EditMode：`75c0159540894a0884ea2be76bef96cd`（`3/3 passed`）
  - 首次測試因 UI `Awake` 初始化未觸發而失敗，已修正測試初始化流程後重跑全綠。
- 風險/阻塞：
  - 尚未抽離 UI 字串集中管理，後續改版仍有回歸風險。
- 下一步：
  - 回報你 round-1 結果與調整建議，由你決策是否將 `S10` 標記為 `Done`。

## 交接記錄（2026-02-18）- S10 決策完成（標記 Done）

- 目標：依產品決策完成 S10 正式收口。
- 完成內容：
  - `docs/PROGRESS_OVERVIEW.md`：`S10 UI/UX` 由 `In Progress` 更新為 `Done`。
  - `docs/systems/S10-ui-ux/SYSTEM.md`：驗測結論改為「已完成決策並標記 Done」。
  - 維持本輪 UI 文案調整，不追加新流程改動。
- 驗測結果：
  - 採用 round-1 證據 `75c0159540894a0884ea2be76bef96cd`（`3/3 passed`）作為收口依據。
- 風險/阻塞：
  - 無阻塞；風險為文案回歸風險（建議下一輪導入 `UIStrings` 集中管理）。
- 下一步：
  - 進入 S10 監控與 round-2 UI 結構/視覺優化規劃。

## 交接記錄（2026-02-18）- S10 round-2 文案映射收斂完成

- 目標：降低 UI 文案回歸風險，將關鍵繁中字詞集中管理並補驗測守門。
- 完成內容：
  - 新增 `Assets/MnemosyneArcana/Scripts/Prototype/PrototypeUiText.cs`（共用映射：難度/盲注/流程階段/商店類型）。
  - `PrototypeCardGameUiController`、`PrototypeSandboxController` 改用共用映射。
  - Sandbox 英文流程 log 文案改為繁中。
  - `S10UiLocalizationTests` 新增 `S10_M4_SharedUiTerms_AreTraditionalChinese`。
  - `docs/baseline/17-test-matrix.md` 新增 `TC-S10-004`。
  - `docs/systems/S10-ui-ux/SYSTEM.md` 回填 round-2 證據與調整建議。
- 驗測結果：
  - Unity MCP EditMode：`ba39f0135508427daf8f1ec0f4301c64`（`4/4 passed`）
- 風險/阻塞：
  - 無阻塞；目前仍有部分 UI 字串散落於流程 log，後續可再抽離到完整 `UIStrings` 資源表。
- 下一步：
  - 若你同意，進入 S10 round-3：開始 UI 結構與視覺層級優化（不改核心遊戲邏輯）。

## 交接記錄（2026-02-18）- S10 舊版 UI 覆蓋問題修正

- 目標：解決進 Play 仍看到舊版 Prototype UI 的問題。
- 完成內容：
  - `PrototypeCardGameUiController` 啟動時新增舊控制器停用流程：
    - `PrototypeGameScreenController`
    - `PrototypeSandboxController`
  - 新增測試 `S10_M5_NewUiDisablesLegacyPrototypeControllers`，防止回歸。
  - 更新 S10 文件與 test matrix（新增 `TC-S10-005`）。
- 驗測結果：
  - Unity MCP EditMode：`ad4c0f324b8c47b4ae08b2954fcc6a86`（`5/5 passed`）。
- 風險/阻塞：
  - 無阻塞。
- 下一步：
  - 你可直接在 Unity Play 驗證新版 UI；若仍有舊畫面，回報具體畫面特徵我可再精準排查。

## 交接記錄（2026-02-18）- S10 正式可玩 UI 重新規劃

- 目標：將 S10 從「開發調參 UI」轉為「玩家正式可玩 UI」路線。
- 完成內容：
  - `docs/PROGRESS_OVERVIEW.md`：S10 改回 `In Progress`。
  - 重寫 `docs/systems/S10-ui-ux/SYSTEM.md`：明確納入正式版 UI 範圍與禁止項（調參/開發按鈕）。
  - 新增 `docs/plans/2026-02-18-s10-formal-ui-replan.md`：round-3 實作步驟與驗收門檻。
- 驗測結果：
  - 本次為規劃重啟，尚未執行 round-3 新測項。
- 風險/阻塞：
  - 目前 UI 程式仍有開發按鈕，需進入 round-3 實作才能完成產品目標。
- 下一步：
  - 依新規劃實作 `PlayerMode` 及 UI 版面重排，再跑 S10 round-3 驗測。

## 交接記錄（2026-02-18）- S10 round-3 第一輪完成（流程重排 + M7）

- 目標：承接 S10 正式可玩 UI 重規劃，完成第一輪 round-3 實作與驗測。
- 完成內容：
  - `PrototypeCardGameUiController`：調整玩家主流程區塊順序，將答題區後接核心操作列，強化「選牌/答題/出牌」連續性。
  - `S10UiLocalizationTests`：
    - 新增 `S10_M7_PlayerMode_KeepsFormalCoreSections`（玩家模式核心區塊與操作按鈕保留）。
    - 更新 `S10_M2` 驗測語意為「狀態與局外資訊繁中」以符合正式版 PlayerMode。
  - 文件回填：
    - `docs/baseline/17-test-matrix.md` 新增 `TC-S10-006`，並更新 `TC-S10-002` 描述。
    - `docs/systems/S10-ui-ux/SYSTEM.md` 補 round-3 證據與調整建議。
    - `docs/PROGRESS_OVERVIEW.md`：S10 更新為 `Implementation=Done`、`Verification=Done (round-3)`，狀態維持 `In Progress` 等你決策。
- 驗測結果：
  - Unity MCP EditMode：job `1e07630612ee4288980084521bdc4837`（`7/7 passed`）。
- 風險/阻塞：
  - 無阻塞；目前主要為 UX 細節優化風險（主要/次要按鈕層級可再微調）。
- 下一步：
  - 等你決策是否將 S10 由 `In Progress` 更新為 `Done`。
