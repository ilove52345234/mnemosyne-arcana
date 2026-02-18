# S1 - Run/Blind 狀態機與推進

## 1. 設計規劃
- 目標：任何一局都能在「可失敗、可恢復、可重開」的節奏中完成完整循環。
- 核心體驗：快進快出，失敗是資訊回饋，不是流程阻塞。

## 2. 規格文件
- Run 由 8 個 Ante 組成，每個 Ante 包含 Small / Big / Boss。
- 核心相位：Boot -> RunStart -> BlindStart -> HandSelect -> HandResolve -> BlindResult -> Shop -> AnteAdvance -> BossResolve -> RunComplete / RunFail。
- 通關分流：
- 達標：BlindResult -> Shop
- 未達標且出牌耗盡：BlindResult -> RunFail
- Ante8 Boss 達標：RunComplete
- 盲注目標採前緩後陡曲線（Ante1~8）。
- 商店後推進規則固定：Small -> Big -> Boss -> 下一 Ante 的 Small。

## 3. 實作紀錄
- 已完成狀態機主流程與盲注分流。
- 已完成 Alpha A-01 全流程回歸路徑（通關與失敗分支）。

## 4. 驗測報告與調整建議
- 驗測結論（2026-02-18，重啟 S1）：驗測完成，`Done` 待你決策。
- 三模型對應：
  - `M-Low`：`RunFlowTests.ResolveBlindResult_WhenFail_EndsRun`（失敗分支可收斂到 RunFail）。
  - `M-Mid`：`RunFlowTests.ResolveBlindResult_WhenPass_EntersShop` + `StartRun_InitializesAnte1SmallBlind`（標準流程可推進）。
  - `M-High`：`RunFlowTests.StartRun_ChallengingProfile_HasHigherTargetThanStandard` + `AlphaRegressionTests.Ante1To8_AllBlindsPass_ReachesRunComplete`（高壓與全通關路徑）。
- 失敗/邊界案例：
  - `RunFlowTests.SubmitHandScore_NegativeScore_ReturnsInvalidInput`
  - `AlphaRegressionTests.Ante3BigBlind_FailPath_EntersRunFail`
- 證據（MCP job）：
  - `e02540ea27314d9cbdc88c3c8cda3298`（RunFlowTests：9/9）
  - `3f9ccd60230741b3ab249f25d5dd300a`（AlphaRegressionTests：2/2）
  - `b7170cb97e764ea3b21e9d23f38da8b5`（PlayableLoopUseCaseTests：2/2）
- 重跑證據（MCP job，2026-02-18）：
  - `fed23d26c31f4f3f9c56534870e20d5c`（RunFlowTests：9/9）
  - `4063f64f1eeb4e289fcc005bfc1705b8`（AlphaRegressionTests：2/2）
  - `8411e497ec244c5db612c8e6a8d7f46c`（PlayableLoopUseCaseTests：2/2）
- 本輪設計問題：
1. 本輪未觀察到新的設計問題（流程、分流、收斂行為均符合規格）。
- 調整建議（小幅）：
1. 若新手失敗後重開節奏偏慢，可微降 Ante1 Big 目標分 3~5%（僅 Standard）。
2. 若高壓檔位過於平滑，可微升 Challenging Ante2 Big/Boss 2~3% 做體感分層。
3. 維持狀態機轉移不動，優先只調分數曲線倍率。

## 5. 更新紀錄
- 2026-02-18：改為系統自洽文件，不再使用跨文件引用描述。
- 2026-02-18：完成 S1 首輪行為驗測，達成 Done 門檻（Low/Mid/High + failure case + job evidence）。
- 2026-02-18：依新規則重啟 S1 驗測，補充「本輪設計問題/調整建議/待你決策 Done」與重跑證據。
