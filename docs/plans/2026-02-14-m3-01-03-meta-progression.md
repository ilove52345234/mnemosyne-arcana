# M3-01~03 局外迴圈實作計畫（已完成）

> 日期：2026-02-14
> 狀態：Done

## 1. 目標範圍

- M3-01：XP/LP 結算
- M3-02：契約生成與結算
- M3-03：契約 LP 45% 上限守門

## 2. 實作摘要

### 2.1 M3-01 XP/LP 結算

- `MetaManagerV2.SettleRun`：
- `XP = highestAnte * 20 + (clear ? 50 : 0)`
- `LP(base) = highestAnte * 2 + (clear ? 5 : 0)`

### 2.2 M3-02 契約生成與結算

- 擴充 `Contract`：`contractType`、`tier`、`lpReward`。
- `GenerateContracts(meta, seed)`：固定契約池，以 seed 決定論抽 3 張。
- `SettleContract(contract, telemetry)`：依 `ContractCompleted` 回傳原始 LP。

### 2.3 M3-03 45% 上限

- 新增 `SettleContractWithCap(contract, telemetry, lpBase)`。
- 上限公式：`cap = floor(lpBase * 45 / 55)`。
- 輸出 `LpBonusRaw`、`LpBonusCapped`、`CapApplied`。

## 3. 測試覆蓋

- `MetaManagerTests`：
- XP/LP 公式（通關/失敗/邊界）
- 契約 seed 決定論與欄位合法性
- 契約完成/失敗結算
- 45% cap 未觸發與觸發案例

## 4. 交付檔案

- `Assets/MnemosyneArcana/Scripts/Core/Contracts/DomainModels.cs`
- `Assets/MnemosyneArcana/Scripts/Core/Contracts/ServiceInterfaces.cs`
- `Assets/MnemosyneArcana/Scripts/Core/Managers/MetaManagerV2.cs`
- `Assets/MnemosyneArcana/Tests/EditMode/MetaManagerTests.cs`
- `docs/IMPLEMENTATION_STATUS.md`
- `docs/SESSION_NOTES.md`

## 5. 已知缺口

- `TryUnlockNode`（M3-04）仍為 `NotImplemented`。
- 契約生成目前為固定池抽樣，尚未完整落實 40/40/20 權重策略。

## 6. 下一步

- 進入 `M3-04`：課程樹 MVP 串接（解鎖、互斥、LP 扣除與持久化）。
