# S6 - Meta/Contract/Curriculum

## 1. 設計規劃
- 目標：局外穩定成長，局內仍保留策略風險。
- 核心原則：契約是加速器，不可壓過主循環。

## 2. 規格文件
- 局外資源：XP（等級）與 LP（解鎖）。
- 結算：每局給基礎 XP/LP，契約可給額外 LP。
- 契約上限：契約 LP 不超過當局總 LP 的 45%。
- 課程樹：4 分支 * 12 層，含前置與互斥。
- 詞庫層級：依覆蓋率、局數與 LP 成本解鎖。

## 3. 實作紀錄
- 已完成 XP/LP 結算與契約生成/結算。
- 已完成 LP cap 守門。
- 已完成課程樹完整 `4x12` 層級規則落地（含 3/6/10 層 A/B 互斥，總 60 節點定義）。
- 已新增課程樹「全節點自動掃描驗測」（逐節點驗證可解鎖/前置失敗/互斥失敗與節點總數）。
- 已新增 `CurriculumEffectSnapshot` 與 `GetCurriculumEffects`，建立節點效果聚合層。
- 已將可落地效果接入既有流程：
  - Learning：時限加成、答錯懲罰緩和、重答折扣。
  - Shop：首重擲折扣、契約後重擲重置、教材價折扣。
  - Scoring：Lv4 額外籌碼、Lv4 前兩張倍率加成、Lv4 build 乘算加成。
  - Decay：Lv4 退化保護。
  - Meta Contract：Learning/Mastery 契約 LP 加成（cap 前套用）。

## 4. 驗測報告與調整建議
- 驗測結論（2026-02-18，重啟 S6）：驗測完成，`Done` 待你決策。
- 三模型對應：
  - `M-Low`：`MetaManagerTests`（含 invalid input、mutex/prereq fail 等邊界；18/18）。
  - `M-Mid`：`UserStoryAcceptanceTests.US10_ContractGenerationIsDeterministicAndSupportsSingleRefresh`（契約生成與刷新可預測）。
  - `M-High`：`UserStoryAcceptanceTests.US11_CurriculumNodeMutexAndPrereqAreEnforced` + `PlayableLoopUseCaseTests.UseCase_CompleteRunAndSettleMeta_ContractRatioWithin45Percent`（課程樹守門＋整局結算 cap）。
- 失敗/邊界案例：
  - `MetaManagerTests.TryUnlockNode_MutexConflict_ReturnsStateConflict`
  - `MetaManagerTests.SettleRun_NullRunResult_ReturnsInvalidInput`
- 重跑證據（MCP job，2026-02-18）：
  - `d86cb3382ed447d2b062ef1842a1c06b`（MetaManagerTests：18/18）
  - `a0bb56146bb143bfb7eed5bd7ae73595`（US10：1/1）
  - `939d297e24264b6c9fa49a5f0c21a278`（US11：1/1）
  - `e57d5aa8a3c342b0b7f4ef42f3da8d58`（PlayableLoop Meta 結算：1/1）
  - `b57c70a89dcc4a16a31387df57eafc22`（MetaManagerTests：20/20，含全節點掃描）
  - `7fe20a299bc040cca8f7dd5086a20524`（US10：1/1）
  - `91acddec900647b59c655fd9f5579758`（US11：1/1）
  - `7f56fe565996497f913183a6b3e643e5`（PlayableLoop Meta 結算：1/1）
  - `e5b60b6634114ff7a4c1bf086ba914bd`（MetaManagerTests：20/20，4x12 完整實作後回歸）
  - `d0f61d4621a74475904103b7c39cd8a8`（US10：1/1）
  - `c7b3dca39190435bb0723049d880450a`（US11：1/1）
  - `9ee3d380c0864701b9e6f19f0d36fc29`（PlayableLoop Meta 結算：1/1）
- 本輪設計問題：
1. 雖已完成節點解鎖與效果聚合，但仍有部分節點效果僅停留在「聚合層」，尚未完整接入對應運行系統（例如詞庫滴入品質、課程卡返還、契約需求減免等）。
- 調整建議：
1. 進入 S6-Effect 第二批：優先補「詞庫/滴詞/契約需求」三類效果入場，避免只在聚合層生效。
2. 新增「節點效果覆蓋率」指標（已聚合/已入場/已驗測）作為 S6 Done 必要條件。
3. 契約 LP cap（45%）持續維持不變，先以完成率與需求側調整體感。

## 5. 更新紀錄
- 2026-02-18：改為系統自洽文件，不再使用跨文件引用描述。
- 2026-02-18：依新規則重啟 S6 驗測，補充「本輪設計問題/調整建議/待你決策 Done」與重跑證據。
- 2026-02-18：新增全節點自動掃描驗測（目前實作定義節點全覆蓋），`MetaManagerTests` 擴充為 20/20。
- 2026-02-18：完成課程樹 `4x12` 全量節點實作（總 60 節點）並完成回歸驗測。
- 2026-02-18：新增節點效果聚合層並接入 Learning/Shop/Scoring/Decay/Meta Contract 第一批運行效果與測試。
