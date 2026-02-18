# 09 - 非功能需求與品質門檻

## 1. 目的

本文件定義「可上線品質」的非功能規格，避免僅有玩法文件而缺乏工程品質基準。

## 2. 目標平台與版本

- 主要平台：PC（開發驗證）
- 次要平台：iOS / Android（結構先對齊）
- Unity 版本：LTS（專案鎖定於 `.unity-version` 或 README 指定）

## 3. 效能門檻

### 3.1 執行時

| 指標 | 目標值 | 驗收方式 |
|---|---:|---|
| 平均 FPS | >= 60（中階手機） | 連續 10 分鐘壓測 |
| 99th frame time | <= 33ms | Profiler 取樣 |
| 主迴圈 GC Alloc | 0B/frame（戰鬥核心） | Profiler Deep Profile |
| 進關載入時間 | <= 2.0s（Warm） | 5 次平均 |

### 3.2 記憶體

| 指標 | 目標值 |
|---|---:|
| 常駐記憶體（中階機） | <= 450MB |
| 單次峰值突增 | <= 80MB |
| 紋理與音訊載入抖動 | 不可造成明顯卡頓 |

## 4. 穩定性門檻

- Run 流程從 Ante1 到 Ante8 不可有 blocker crash。
- 存檔/讀檔成功率：> 99.9%（壓測 1000 次）。
- migration 失敗可回退，不可毀損原檔。

## 5. 正確性門檻

- 分數計算 deterministic（相同 seed + 相同輸入，結果一致）。
- 契約加成 LP 不可超過總 LP 45%。
- 互斥節點不可同時啟用。

## 6. 可維護性門檻

- Domain logic 單元測試覆蓋率目標 >= 80%。
- 每個 manager 需有責任界線，禁止跨層直接改寫他層狀態。
- 所有 JSON schema 有欄位註解與範例。

## 7. 可觀測性（Observability）

最低事件紀錄：
- run_start / run_end
- blind_start / blind_result
- hand_scored（含牌型、分數拆解）
- contract_selected / contract_settled
- save_migrated / save_migration_failed

紀錄格式：結構化 JSON log（含 `runId`, `playerId(hash)`, `buildVersion`）。

## 8. 品質關卡（Quality Gates）

PR 合併前必須通過：
1. 單元測試
2. 主要整合測試（完整 Run）
3. schema 驗證
4. 靜態檢查（lint/format）
5. 無高嚴重度 TODO（如 crash risk）
