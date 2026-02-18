# S2 - Scoring/HandType 得分與牌型

## 1. 設計規劃
- 目標：分數可理解、可預測、可校準。
- 核心原則：同輸入同 seed 必須 deterministic。

## 2. 規格文件
- 基本公式：
- FinalScore = (BaseHandChips + CardChipsTotal) * (BaseHandMult + AdditiveMult) * Multipliers
- 答錯不破牌型，僅降低收益：
- 該卡籌碼 50%
- 牌型倍率 -1（最低 1）
- 牌型集合（10 種）：單字、同性對、同族對、三同性、語序鏈、同族三、滿堂、同族花、全同性、語序同族。
- 單卡籌碼依字長給值，牌型升級依成長表增幅。

## 3. 實作紀錄
- 已落地 HandType 判定優先序與分數拆解輸出。
- 已落地升級成長值與答錯懲罰整合。

## 4. 驗測報告與調整建議
- 驗測結論（2026-02-18，重啟 S2）：驗測完成，`Done` 待你決策。
- 三模型對應：
  - `M-Low`：`ScoringFormulaTests.EvaluateHand_AppliesWrongAnswerPenalty`（答錯降益、倍率下限保護）。
  - `M-Mid`：`ScoringHandTypeTests.EvaluateHand_A_N_V_IsGrammarChain`、`...ThreePlusTwoPos_IsFullHouse`（常見牌型判定）。
  - `M-High`：`ScoringFormulaTests.EvaluateHand_AppliesFullFormulaWithModifiers`（乘算因子與高分輸出）、`...AppliesHandUpgradeGrowth`（升級成長值）。
- 失敗/邊界案例：
  - `ScoringFormulaTests.EvaluateHand_AppliesWrongAnswerPenalty`（`effectiveHandMult` 下限為 1）。
  - `ScoringHandTypeTests.EvaluateHand_N_A_V_IsNotGrammarChain`（逆序不誤判為語序鏈）。
- 證據（MCP job）：
  - `f50a4925d23d42bfb5c0a7b61156d052`（ScoringHandTypeTests：6/6）
  - `fb3e219b58fc417fa3c89d0f57905193`（ScoringFormulaTests：3/3）
- 重跑證據（MCP job，2026-02-18）：
  - `d982086b91be4724a2592a7d6e362b06`（ScoringHandTypeTests：6/6）
  - `82a50f25a0f24254ba78c41ee094bdba`（ScoringFormulaTests：3/3）
- 本輪設計問題：
1. 本輪未觀察到新的設計問題（公式一致性、牌型優先序、懲罰下限皆符合規格）。
- 調整建議（小幅）：
1. 若同族系牌型在實戰出現率偏低，可微升同族對/同族三成長值 1 級差距（先 +5 chips 級距試探）。
2. 若高端 build 分數膨脹過快，優先壓乘算因子來源，不直接砍基礎牌型值。
3. 維持「答錯不破牌型」原則，僅調整懲罰幅度與加算/乘算來源密度。

## 5. 更新紀錄
- 2026-02-18：改為系統自洽文件，不再使用跨文件引用描述。
- 2026-02-18：完成 S2 首輪行為驗測，達成 Done 門檻（Low/Mid/High + boundary case + job evidence）。
- 2026-02-18：依新規則重啟 S2 驗測，補充「本輪設計問題/調整建議/待你決策 Done」與重跑證據。
