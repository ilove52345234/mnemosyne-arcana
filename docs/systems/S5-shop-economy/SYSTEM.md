# S5 - Shop/Economy

## 1. 設計規劃
- 目標：低資金仍有選擇，高資金仍需取捨。
- 核心原則：經濟服務 build 決策，不直接保證勝利。

## 2. 規格文件
- 收入：通關獎勵、剩餘出牌/棄牌轉金、利息、特定表現獎勵。
- 支出：訓練、重答、語感/教材/詞綴/課程、重擲。
- 商店結構：商品區 + 養牌區 + 卡包區，Boss 後課程二選一。
- 價格帶固定，重擲為遞增成本。
- Ante 分段權重：前期偏養成，中期偏 build，後期完整池。

## 3. 實作紀錄
- 已完成 offer 生成、購買成功/失敗、Boss 商店固定課程。
- 已完成分段權重與價格合法性檢查。

## 4. 驗測報告與調整建議
- 驗測結論（2026-02-18，重啟 S5）：驗測完成，`Done` 待你決策。
- 三模型對應：
  - `M-Low`：`ShopManagerTests.PurchaseOffer_NotEnoughMoney_FailsGracefully`（低資金/購買失敗邊界）。
  - `M-Mid`：`UserStoryAcceptanceTests.US04_ShopCanGenerateAndPurchaseWithBalanceGuard`（一般商店生成與購買流程）。
  - `M-High`：`UserStoryAcceptanceTests.US08_BossShopAlwaysOffersTwoCoursesAtPrice10`（Boss 商店固定課程）+ `PlayableLoopUseCaseTests.UseCase_FirstBlindToShopPurchaseAndAdvance_Works`（實際循環推進）。
- 失敗/邊界案例：
  - `ShopManagerTests.PurchaseOffer_NotEnoughMoney_FailsGracefully`
- 重跑證據（MCP job，2026-02-18）：
  - `a0d29454c3bc492591473af1ad7ba3d4`（ShopManagerTests：6/6）
  - `a10d2e2fa9524fdba1e4d2128f01dd43`（US04：1/1）
  - `ab43dafa9a89430892776bc02a0d6d5a`（US08：1/1）
  - `c0fa496671614d348a9bd9b676f14650`（PlayableLoop 商店流程：1/1）
- 本輪設計問題：
1. 重擲成本遞增的「長局資金壓力曲線」仍缺系統級驗測證據，現有案例偏短局路徑。
- 調整建議：
1. 新增 S5 專屬長局測項（至少 20 次重擲）觀察資金耗損率與可購買回合比例。
2. 若低資金可玩性不足，先將 early ante 常規商品最低價位下修 1 金，不改 Boss 課程價。
3. 若中後期過度重擲，重擲遞增斜率可由線性改為「前緩後陡」分段。

## 5. 更新紀錄
- 2026-02-18：改為系統自洽文件，不再使用跨文件引用描述。
- 2026-02-18：依新規則重啟 S5 驗測，補充「本輪設計問題/調整建議/待你決策 Done」與重跑證據。
