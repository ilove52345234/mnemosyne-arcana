# 22 - A-01 全流程回歸清單（Ante 1-8）

> 目的：定義 Alpha Gate A-01 的執行步驟與驗收證據。

## 1. 回歸範圍

- 難度：`Standard`（SoT 基線）
- 流程：Ante1 Small -> Big -> Boss ... -> Ante8 Boss
- 模組：`RunManagerV2` + `ScoringManagerV2` + `ShopManagerV2` + `LearningManagerV2` + `MetaManagerV2`

## 2. 自動化測試（已完成）

- `Assets/MnemosyneArcana/Tests/EditMode/AlphaRegressionTests.cs`
- `Ante1To8_AllBlindsPass_ReachesRunComplete`
- `Ante3BigBlind_FailPath_EntersRunFail`
- 執行指令：
  - `UNITY_PATH='/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity' bash scripts/run_editmode_tests.sh`

## 3. 手動/整合驗證步驟

1. 啟動可授權 Unity 環境
2. 執行 EditMode tests（含 AlphaRegressionTests）
3. 逐項記錄：
- 是否可完成 Ante 1-8
- 中途是否有死狀態（無法推進）
- 失敗分支是否正確進入 `RunFail`
- 最終 Boss 是否正確進入 `RunComplete`

## 4. 驗收結果記錄（2026-02-16）

| 檢查項 | 結果 | 證據 |
|---|---|---|
| Ante1-8 通關流程 | Pass | `AlphaRegressionTests.Ante1To8_AllBlindsPass_ReachesRunComplete` |
| 失敗分支流程 | Pass | `AlphaRegressionTests.Ante3BigBlind_FailPath_EntersRunFail` |
| 死狀態檢查 | Pass | `run_editmode_tests.sh` 結果 `[OK] EditMode tests finished.` |
| 事件/狀態一致性 | Pass | EditMode 測試全綠 + `RunComplete`/`RunFail` 斷言通過 |

## 5. 阻塞狀態

- 已解除：可授權 Unity 環境可執行 batchmode 與 EditMode 測試。
- 已解除：`MetaManagerV2` 的 `IsExternalInit` 編譯錯誤已修正（commit `1311b71`）。
- 後續非阻塞工作：A-02（存檔/migration）與 A-03（效能穩定性）驗收。
