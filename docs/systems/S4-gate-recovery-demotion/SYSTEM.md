# S4 - Gate/Recovery/Demotion

## 1. 設計規劃
- 目標：卡關合理、回補可行、退回可控。
- 核心原則：不能靠短期運氣越級，也不能無限打轉。

## 2. 規格文件
- 有效詞彙量：EffectiveVocab = LearnedCount * RetentionRate * RetrievalRate。
- 通關守門條件：覆蓋率與主動回憶表現需同時達標。
- Recovery Gate：先回補，再判定是否回主線。
- Demotion：連續未恢復才退回前關，且有保護窗避免頻繁退關。
- 長週期退化：以 7/14/30 天作為關鍵觀測點，要求趨勢單調。

## 3. 實作紀錄
- 已接入 gate progression 決策與 recovery 流程。
- 已完成長週期分布測試與排序驗證。

## 4. 驗測報告與調整建議
- 最新結果：S4 驗測通過（Priority + LongCycle）。
- 調整建議：
1. 新手挫折偏高時，先微調 recovery 增益，不先動關卡門檻。
2. 高段過快滾雪球時，先微調 demotion 懲罰幅度。

## 5. 更新紀錄
- 2026-02-18：完成 GS-07 驗測一輪（3/3 + 2/2 pass）。
- 2026-02-18：改為系統自洽文件，不再使用跨文件引用描述。
