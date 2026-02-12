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
| TC-CONTRACT-001 | 契約 LP 上限 | `lpBonusCapped <= totalLP * 0.45` |
| TC-LEX-001 | 詞庫層門檻判定 | 未達門檻不可解鎖 |
| TC-META-001 | 互斥節點 | 無法同時啟用互斥節點 |
| TC-MIG-001 | v1->v2 遷移成功 | 生成 `saveVersion=2` 且備份存在 |
| TC-MIG-002 | 遷移失敗回退 | 還原備份，阻擋進 Run |
