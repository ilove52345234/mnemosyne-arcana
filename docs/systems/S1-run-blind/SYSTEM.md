# S1 - Run/Blind 狀態機與推進

## 1. 設計規劃
- 目標：任何一局都能在「可失敗、可恢復、可重開」的節奏中完成完整循環。
- 核心體驗：快進快出，失敗是資訊回饋，不是流程阻塞。

## 2. 規格文件
- Run 由 8 個 Ante 組成，每個 Ante 包含 Small / Big / Boss。
- 核心相位：Boot -> RunStart -> BlindStart -> HandSelect -> HandResolve -> BlindResult -> Shop -> AnteAdvance -> BossResolve -> RunComplete / RunFail。
- 通關分流：
- 達標：BlindResult -> Shop
- 未達標且出牌耗盡：BlindResult -> RunFail
- Ante8 Boss 達標：RunComplete
- 盲注目標採前緩後陡曲線（Ante1~8）。
- 商店後推進規則固定：Small -> Big -> Boss -> 下一 Ante 的 Small。

## 3. 實作紀錄
- 已完成狀態機主流程與盲注分流。
- 已完成 Alpha A-01 全流程回歸路徑（通關與失敗分支）。

## 4. 驗測報告與調整建議
- 現況：流程測試通過，無死鎖路徑。
- 調整建議：
1. 加強「連續失敗後重開」行為驗測批次。
2. 增加高壓詞條下相位轉移一致性檢查。

## 5. 更新紀錄
- 2026-02-18：改為系統自洽文件，不再使用跨文件引用描述。
