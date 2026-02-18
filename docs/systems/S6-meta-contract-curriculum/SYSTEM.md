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
- 已完成 Batch-2：
  - 詞庫層級解鎖門檻計算（LP/局數/覆蓋率折扣）。
  - 滴詞權重計算（退化池與 stale 詞權重加成）。
  - 契約需求減免計算（Mastery 契約需求下修，最低 1）。
- 已完成 Batch-3：
  - 詞庫滴入品質偏向（短詞/長詞）已接入權重計算。
  - 課程卡購買 LP 返還（BLD_09）已接入購買結果。
  - Boss 特化效果（MAS_08：Lv4 首次答錯免額外倍率懲罰）已接入學習結果。
- 已完成 Batch-4：
  - FLU 高體感節點：免費重答（FLU_04）、連錯保底門檻調整（FLU_05）、全對 LP bonus（FLU_07）、連對效果延長（FLU_09）已接入運行 API。
  - BLD 高體感節點：下一次刷新類別預覽（BLD_01）、候選槽位 +1（BLD_02）、訓練折扣（BLD_03A/03B）已接入商店 API。
- 已完成 Batch-5：
  - BLD 進階商店效果：語感權重加成（BLD_06A）、指定機制詞條權重加成（BLD_06B）、養成鎖定帶出槽（BLD_07）、首包保底型別（BLD_10A/10B）已接入商店/Meta API。
  - MAS 進階成長效果：Boss 全對額外 Lv4 升級次數（MAS_07）、首張 Lv4 契約進度加成（MAS_09）、單局 Lv4 次數里程 LP（MAS_11）、結算 Lv4 次數區段 LP（MAS_12）已接入 Learning/Meta API。
- 已完成 Batch-6（全節點補完）：
  - FLU 補完：FLU_03A/03B/06B/12 已接入 runtime API（簡易題率、Lv3 答對獎勵、拼字容錯、首次 Lv4 降級免疫）。
  - LEX 補完：LEX_04/05/06A/06B/07/08/12 已接入 runtime API（弱項詞、缺口補齊、保底 Lv4、退化池優先回補、首次退化詞 LP）。
  - MAS 補完：MAS_03B/04/05 已接入 runtime API（Lv4 負面抗性、5 張 Lv4 里程 LP、Lv3->4 需求下修）。
  - 補齊 BLD runtime API：BLD_08（Ante 首商店券）、BLD_12（首次 Lv4 升級返還）事件入口。
  - 新增「全節點 effect 映射檢查」驗測，要求 60 節點每個至少映射一個 runtime effect 欄位。

## 4. 驗測報告與調整建議
- 驗測結論（2026-02-18，重啟 S6）：全節點實作已補完；本輪已補跑 MCP job 證據，runner 可正常完成回歸驗測；`Done`（已決策同意）。
- 三模型對應：
  - `M-Low`：`MetaManagerTests`（含 invalid input、mutex/prereq fail 等邊界；18/18）。
  - `M-Mid`：`UserStoryAcceptanceTests.US10_ContractGenerationIsDeterministicAndSupportsSingleRefresh`（契約生成與刷新可預測）。
  - `M-High`：`UserStoryAcceptanceTests.US11_CurriculumNodeMutexAndPrereqAreEnforced` + `PlayableLoopUseCaseTests.UseCase_CompleteRunAndSettleMeta_ContractRatioWithin45Percent`（課程樹守門＋整局結算 cap）。
- 本輪 Batch-5 對應驗測：
  - `M-Low`：`MetaManagerTests`（30/30）、`ShopManagerTests`（17/17）、`LearningManagerTests`（16/16）。
  - `M-Mid`：`UserStoryAcceptanceTests.US10_ContractGenerationIsDeterministicAndSupportsSingleRefresh`（1/1）。
  - `M-High`：`UserStoryAcceptanceTests.US11_CurriculumNodeMutexAndPrereqAreEnforced`（1/1） + `PlayableLoopUseCaseTests.UseCase_CompleteRunAndSettleMeta_ContractRatioWithin45Percent`（1/1）。
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
  - `1fd370656f7e4ffbb03a17a803fe0385`（MetaManagerTests：25/25，Batch-2）
  - `b8db07432ff44c54945f2696b75e4843`（US10：1/1）
  - `087792093ff746f88d00c80a7ac0ca7a`（US11：1/1）
  - `b824cb7150884281ad6d0018ca51e59e`（PlayableLoop Meta 結算：1/1）
  - `b0544fe5f9684eee8d517d9c23d9682b`（MetaManagerTests：26/26，Batch-3）
  - `7b1190a67a2b460a891817e4bb0b015d`（LearningManagerTests：12/12）
  - `86e9cb2cfe7b4ea1a5b12d24d3891ca8`（ShopManagerTests：11/11）
  - `0fc0bccf892a47f691f89011ab783d5d`（US10：1/1）
  - `34f1896e1c2a4e9d9ea762184ce16a2f`（US11：1/1）
  - `9a8358169f2145cfbbc7faeec1769d79`（PlayableLoop Meta 結算：1/1）
  - `96a24864d4214fe29831a810909499dd`（MetaManagerTests：27/27，Batch-4）
  - `51925efcc42144bd84e14365459cc6b1`（LearningManagerTests：15/15）
  - `87fb9636e5f645dc92939d33807e71fa`（ShopManagerTests：14/14）
  - `788a39b266914d80a87716052eaa5237`（US10：1/1）
  - `2a9a379e57674c579b6801b1dd20b709`（US11：1/1）
  - `dcf2f433b3a14512b27506b461b1d00e`（PlayableLoop Meta 結算：1/1）
- 本輪重跑證據（MCP job，2026-02-18，Batch-5）：
  - `4571e04a11434ea9a8da64ac4c174df6`（MetaManagerTests：30/30）
  - `7194e6ab373747fcaad95cf662d10d07`（ShopManagerTests：17/17）
  - `037ab385d0e64e6386c00718932bf895`（LearningManagerTests：16/16）
  - `ad69bfaa6c5c411799eea33ef7816a4b`（US10：1/1）
  - `a4a067ee6ac8407d8b7269c537c8242d`（US11：1/1）
  - `42eb6672a03a46108357fc9778bedcb9`（PlayableLoop Meta 結算：1/1）
- 本輪補強證據（MCP job，2026-02-18）：
  - `dae7c0b1e5dc47f5a6a8b1c82b9ce218`（EditMode：175/175，含 `MetaManagerTests`、`US10`、`US11`、`PlayableLoopUseCaseTests.UseCase_CompleteRunAndSettleMeta_ContractRatioWithin45Percent`）
- 本輪設計問題：
1. 本輪未觀察到新增設計問題；既有節點效果覆蓋率與契約 cap 行為維持穩定。
- 調整建議：
1. 維持「節點效果覆蓋率」為 S6 Done 必要條件，後續新增節點需同步補映射驗測。
2. 契約 LP cap（45%）持續維持不變，先驗證穩定再做數值微調。

## 5. 更新紀錄
- 2026-02-18：改為系統自洽文件，不再使用跨文件引用描述。
- 2026-02-18：依新規則重啟 S6 驗測，補充「本輪設計問題/調整建議/待你決策 Done」與重跑證據。
- 2026-02-18：新增全節點自動掃描驗測（目前實作定義節點全覆蓋），`MetaManagerTests` 擴充為 20/20。
- 2026-02-18：完成課程樹 `4x12` 全量節點實作（總 60 節點）並完成回歸驗測。
- 2026-02-18：新增節點效果聚合層並接入 Learning/Shop/Scoring/Decay/Meta Contract 第一批運行效果與測試。
- 2026-02-18：完成 Batch-2（詞庫解鎖門檻、滴詞權重、契約需求減免）與對應測試，`MetaManagerTests` 擴充為 25/25。
- 2026-02-18：完成 Batch-3（滴詞品質偏向、課程卡 LP 返還、MAS_08 Boss 特化）與對應測試，`MetaManagerTests` 擴充為 26/26。
- 2026-02-18：完成 Batch-4（FLU_04/05/07/09、BLD_01/02/03A/03B）效果入場與測試，`MetaManagerTests` 擴充為 27/27。
- 2026-02-18：完成 Batch-5（BLD_06A/06B/07/10A/10B、MAS_07/09/11/12）效果入場與測試，`MetaManagerTests` 擴充為 30/30，`ShopManagerTests` 擴充為 17/17，`LearningManagerTests` 擴充為 16/16。
- 2026-02-18：完成 Batch-6（全節點補完：FLU/LEX/MAS 剩餘節點 + BLD 補充事件 API），新增全節點 effect 映射驗測；目前 `run_tests` 受 runner busy 影響，待恢復後補 MCP job 證據。
- 2026-02-18：runner 恢復後完成補跑，新增 MCP job `dae7c0b1e5dc47f5a6a8b1c82b9ce218`（EditMode 175/175）作為 S6 收口證據。
