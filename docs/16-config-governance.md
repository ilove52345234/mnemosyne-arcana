# 16 - 設定檔治理規範

## 1. 目的

建立 JSON 設定檔的命名、驗證、版本與審核流程，降低配置錯誤導致的線上風險。

## 2. 目錄與命名規範

建議目錄：
- `Assets/Resources/Data/V2/`

命名：
- `hand_types.v2.json`
- `blind_curve.v2.json`
- `shop_pool.v2.json`
- `curriculum_tree.v2.json`
- `contracts.v2.json`

規則：
- 檔名小寫 snake_case
- 版本明確寫在檔名與內容（`schemaVersion`）

## 3. Schema 驗證流程

1. 變更 JSON
2. 跑 schema 驗證
3. 跑引用完整性檢查（ID 是否存在）
4. 跑平衡護欄檢查（LP cap、互斥規則）

## 4. 版本升級規則

- 小改動（向後相容）：`schemaVersion` 不變，新增 optional 欄位
- 破壞性改動：`schemaVersion +1`，附 migration

## 5. 審核守門（PR Gate）

PR 若含設定變更，必須提供：
1. 變更摘要
2. 影響範圍（哪個系統）
3. 回歸結果
4. 回滾方案

## 6. 最低工具要求

- schema validate 命令
- config lint 命令
- ID 交叉引用檢查命令

（可於 M0 建立 `scripts/validate_configs.*`）
