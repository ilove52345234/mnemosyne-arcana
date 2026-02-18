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
- 已完成課程樹 MVP 解鎖守門（前置/互斥/成本）。
- 已新增課程樹「全節點自動掃描驗測」（逐節點驗證可解鎖/前置失敗/互斥失敗）。

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
- 本輪設計問題：
1. 課程樹已覆蓋「目前實作定義的全部節點」；但與主規格 `4x12` 仍有範圍落差（現實作為 MVP 4 分支前 3 層）。
- 調整建議：
1. 若你要以 S6 正式 Done 收斂，建議先確認「Done 判定以現行 MVP 範圍」還是「必須先補齊 4x12」。
2. 契約 LP cap（45%）目前行為正確；若後續想提高契約體感，先調契約完成率，不先放寬 cap。
3. 詞庫層級解鎖建議補長局節奏模擬，避免中後期解鎖過密或過疏。

## 5. 更新紀錄
- 2026-02-18：改為系統自洽文件，不再使用跨文件引用描述。
- 2026-02-18：依新規則重啟 S6 驗測，補充「本輪設計問題/調整建議/待你決策 Done」與重跑證據。
- 2026-02-18：新增全節點自動掃描驗測（目前實作定義節點全覆蓋），`MetaManagerTests` 擴充為 20/20。
