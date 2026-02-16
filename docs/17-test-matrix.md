# 17 - 測試矩陣（需求對測試）

## 1. 目的

將需求、測試型別、驗收責任對齊，避免測試範圍遺漏。

## 2. 測試矩陣

| 需求 | 單元測試 | 整合測試 | 回歸測試 | 主要責任模組 |
|---|---|---|---|---|
| 牌型判定正確 | 是 | 是 | 是 | ScoringManagerV2 |
| 分數公式 deterministic | 是 | 是 | 是 | ScoringManagerV2 |
| 答錯不斷牌型 | 是 | 是 | 是 | LearningManagerV2 + RunManagerV2 |
| 盲注通關/失敗流程 | 否 | 是 | 是 | RunManagerV2 |
| 商店抽樣與購買 | 是 | 是 | 是 | ShopManagerV2 |
| 退化規則（1/3/7 天） | 是 | 是 | 是 | DecayManagerV2 |
| Boss 學習規則 | 是 | 是 | 是 | LearningManagerV2 |
| 契約 3 選 1 與結算 | 是 | 是 | 是 | MetaManagerV2 |
| 契約 LP <=45% | 是 | 是 | 是 | MetaManagerV2 |
| 詞庫層級解鎖門檻 | 是 | 是 | 是 | MetaManagerV2 |
| 互斥節點限制 | 是 | 是 | 是 | MetaManagerV2 |
| v1->v2 migration | 是 | 是 | 是 | Save/Migration Service |

## 3. 測試資料策略

- 固定 seed 測試：驗證 deterministic
- 邊界值測試：0、上限、負值、空資料
- 錯誤注入測試：缺欄位、未知 enum、版本不符

## 4. CI 最低測項

1. 單元測試全跑
2. 主要整合流程（Ante1->Boss）
3. 設定檔 schema 驗證
4. migration 測試

## 5. 出關標準

- P0 功能測試全綠
- 無高嚴重度缺陷
- 回歸結果可重現


## 6. 測試案例 ID（首批）

| Case ID | 需求 | 預期結果 |
|---|---|---|
| TC-SCORE-001 | 同輸入同 seed 分數一致 | `finalScore` 完全相同 |
| TC-SCORE-002 | 答錯不斷牌型 | `handType` 不變，`finalScore` 下降 |
| TC-SCORE-003 | 教材升級成長值生效 | `upgradedHandChips/Mult` 依 SoT 成長 |
| TC-SCORE-004 | 答錯懲罰下限 | `effectiveHandMult >= 1` |
| TC-RUN-001 | 小盲注通關進商店 | `nextPhase = Shop` |
| TC-RUN-002 | 出牌次數耗盡仍未達標 | `nextPhase = RunFail` |
| TC-RUN-003 | Boss Ante8 達標 | `nextPhase = RunComplete` |
| TC-RUN-004 | 商店後推進盲注 | `Small -> Big -> Boss -> next Ante Small` |
| TC-RUN-005 | Standard 曲線基線 | Ante1 Small 目標分 = 100 |
| TC-RUN-006 | Relaxed 曲線 | 同條件下目標分 < Standard |
| TC-RUN-007 | Challenging 曲線 | 同條件下目標分 > Standard |
| TC-SHOP-001 | 商店 seed 決定論 | 同 seed 同 ante 產出相同 offer 與價格 |
| TC-SHOP-002 | Ante1 不出課程卡 | `category != Course` |
| TC-SHOP-003 | 購買成功扣款 | `remainingMoney = currentMoney - cost` |
| TC-SHOP-004 | 餘額不足 | `success = false` 且保留原餘額 |
| TC-SHOP-005 | Boss 商店課程卡 | 固定 2 張 `Course`，價格皆為 10 |
| TC-SHOP-006 | 價格帶合法性 | 各 category 價格落在 SoT 區間 |
| TC-LEARN-001 | Lv0 答對行為 | 題型/限時/籌碼係數符合 Lv0 並升 Lv1 |
| TC-LEARN-002 | 答錯懲罰輸出 | `chipMultiplier=0.5` 且 `handMultDelta=-1` |
| TC-LEARN-003 | Boss Lv4 視為 Lv3 | `effectiveLevel=Lv3`，題型為拼字 |
| TC-LEARN-004 | 賭一把成功判定 | `GambleSuccess` 視為答對並可升級 |
| TC-LEARN-005 | 答錯選擇-接受損失 | 保留金錢，結果為 `Wrong`，倍率 0.5 |
| TC-LEARN-006 | 答錯選擇-重答 | 扣 $2、只能一次、結果為 `RetryAccepted` |
| TC-LEARN-007 | 答錯選擇-賭一把 | 50% 全回復 / 50% 歸零，seed 可重現 |
| TC-CONTRACT-001 | 契約 LP 上限 | `lpBonusCapped <= totalLP * 0.45` |
| TC-CURR-001 | 課程樹解鎖成功 | 前置滿足且 LP 足夠，返回 `Success=true` |
| TC-CURR-002 | 缺前置節點 | 返回 `StateConflict` |
| TC-CURR-003 | 互斥衝突 | 返回 `StateConflict` |
| TC-CURR-004 | LP 不足 | 返回 `StateConflict` |
| TC-ALPHA-001 | Ante1-8 全通關路徑 | 最終 `phase = RunComplete` |
| TC-ALPHA-002 | 中途失敗路徑 | 目標未達且出牌耗盡 -> `phase = RunFail` |
| TC-LEX-001 | 詞庫層門檻判定 | 未達門檻不可解鎖 |
| TC-META-001 | 互斥節點 | 無法同時啟用互斥節點 |
| TC-MIG-001 | v1->v2 遷移成功 | 生成 `saveVersion=2` 且備份存在 |
| TC-MIG-002 | 遷移失敗回退 | 還原備份，阻擋進 Run |
| TC-DECAY-001 | Lv1 超過 1 天未練 | 退化到 Lv0, Decayed 池 |
| TC-DECAY-002 | Lv2 剛好 3 天未練 | 退化到 Lv1, Decayed 池 |
| TC-DECAY-003 | Lv3 + 6 天未練 | 不退化 |
| TC-DECAY-004 | Lv4 超過 7 天 | 退化到 Lv3, Learning 池 |
| TC-DECAY-005 | Lv0 + 任何時間 | 不退化 |
| TC-DECAY-006 | 答對後重設計時 | `lastPracticed` 更新 |
| TC-DECAY-007 | 批次退化多詞 | 各詞獨立判定 |
| TC-BOSS-001 | Boss + Lv0 | effectiveLevel = Lv1 |
| TC-BOSS-002 | Boss + Lv2 | effectiveLevel = Lv3 |
| TC-BOSS-003 | Boss + Lv3 | effectiveLevel = Lv3（封頂） |
| TC-BOSS-004 | Boss + Lv4 | effectiveLevel = Lv3（回歸） |
| TC-BOSS-005 | 連對 3 題 | chipMultiplier = 2.0 |
| TC-BOSS-006 | 連對 2 題 | chipMultiplier = 1.0（無獎勵） |
| TC-BOSS-007 | Boss 全對 | 打出卡各 +1 等級 |
| TC-BOSS-008 | Boss 全對含 Lv4 | Lv4 不升，skippedAtMax = 1 |
| TC-BOSS-009 | 非 Boss 盲注 | 無等級偏移 |
