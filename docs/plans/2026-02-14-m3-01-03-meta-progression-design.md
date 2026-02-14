# M3-01~03 局外迴圈設計（XP/LP + 契約 + LP 上限）

> 日期：2026-02-14
> 狀態：Approved

## 1. M3-01 XP/LP 結算

### 公式

| 項目 | 公式 |
|---|---|
| XP | `highestAnte * 20` + 通關 `+50` |
| 基礎 LP | `highestAnte * 2` + 通關 `+5` |
| 契約 LP | 由 SettleContract 提供（受 45% cap） |
| 總 LP | `基礎 LP + 契約 LP(capped)` |

## 2. M3-02 契約生成/結算

### GenerateContracts

- 產出 3 張，seed deterministic
- 類型權重：自然 40% / 學習 40% / 風格 20%
- 難度：Tier1 (4-6 LP) / Tier2 (7-10 LP) / Tier3 (11-15 LP)
- MVP：固定契約池 + seed 選取

### SettleContract

- completed 由呼叫方傳入
- 完成：lpBonusRaw = 契約獎勵
- 未完成：lpBonusRaw = 0
- 套 45% cap 後輸出 lpBonusCapped

## 3. M3-03 LP 45% 上限

- 規則：`契約 LP / 總 LP <= 0.45`
- 實作：`lpBonusCapped = min(lpBonusRaw, floor(lpBase * 45 / 55))`
- 確保 `capped / (lpBase + capped) <= 0.45`

## 4. Contract DTO 擴充

| 欄位 | 型別 | 說明 |
|---|---|---|
| contractId | string | 契約 ID |
| name | string | 契約名稱 |
| contractType | string | Natural/Learning/Style |
| tier | int | 1/2/3 |
| lpReward | int | 基礎獎勵 LP |

## 5. 測試案例

| Case ID | 場景 | 預期 |
|---|---|---|
| TC-META-XP-001 | Ante5 通關 | xp=150, lpBase=15 |
| TC-META-XP-002 | Ante3 失敗 | xp=60, lpBase=6 |
| TC-CONTRACT-GEN-001 | seed 決定論 | 同 seed 同契約 |
| TC-CONTRACT-GEN-002 | 產出 3 張 | count=3 |
| TC-CONTRACT-SET-001 | 契約完成 | lpBonusRaw=契約獎勵 |
| TC-CONTRACT-SET-002 | 契約未完成 | lpBonusRaw=0 |
| TC-CONTRACT-CAP-001 | LP 未觸發上限 | capApplied=false |
| TC-CONTRACT-CAP-002 | LP 超過 45% | capApplied=true |
| TC-META-SETTLE-001 | 完整結算 | lpTotal=lpBase+lpContract(capped) |
