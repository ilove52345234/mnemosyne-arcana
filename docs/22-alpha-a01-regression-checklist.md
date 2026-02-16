# 22 - A-01 全流程回歸清單（Ante 1-8）

> 目的：定義 Alpha Gate A-01 的執行步驟與驗收證據。

## 1. 回歸範圍

- 難度：`Standard`（SoT 基線）
- 流程：Ante1 Small -> Big -> Boss ... -> Ante8 Boss
- 模組：`RunManagerV2` + `ScoringManagerV2` + `ShopManagerV2` + `LearningManagerV2` + `MetaManagerV2`

## 2. 自動化測試（已準備）

- `Assets/MnemosyneArcana/Tests/EditMode/AlphaRegressionTests.cs`
- `Ante1To8_AllBlindsPass_ReachesRunComplete`
- `Ante3BigBlind_FailPath_EntersRunFail`

## 3. 手動/整合驗證步驟

1. 啟動可授權 Unity 環境
2. 執行 EditMode tests（含 AlphaRegressionTests）
3. 逐項記錄：
- 是否可完成 Ante 1-8
- 中途是否有死狀態（無法推進）
- 失敗分支是否正確進入 `RunFail`
- 最終 Boss 是否正確進入 `RunComplete`

## 4. 驗收結果記錄（待填）

| 檢查項 | 結果 | 證據 |
|---|---|---|
| Ante1-8 通關流程 | Pending | - |
| 失敗分支流程 | Pending | - |
| 死狀態檢查 | Pending | - |
| 事件/狀態一致性 | Pending | - |

## 5. 阻塞

- 本環境目前仍有 Unity licensing IPC 限制，無法直接完成 batchmode 驗收。
- 需在可授權 runner 或本機授權正常環境執行最終 A-01。
