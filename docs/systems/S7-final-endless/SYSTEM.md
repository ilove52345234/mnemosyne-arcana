# S7 - Final Gate/Endless

## 1. 設計規劃
- 目標：區分「可完成」與「真精通」。
- 核心原則：Main Clear 與 True Clear 分離，避免短期運氣誤判精通。

## 2. 規格文件
- Main Clear：mastery >= 95%。
- True Clear：mastery = 100% 且 stableDays >= 7。
- Endless：通關後開啟長局模式，觀察穩定性與非法狀態轉移。
- 驗測模型：Low / Mid / High + Edge（長局多 seed）。

## 3. 實作紀錄
- 已落地 final gate 決策 API。
- 已加入 S7 驗測組與 30 seeds 長局模擬。

## 4. 驗測報告與調整建議
- 最新結果：GS-08 驗測通過（4/4 pass）。
- 調整建議：
1. 先不改 95/100+7d 核心門檻。
2. 若需調難度，優先調整高模型成長增益參數。

## 5. 更新紀錄
- 2026-02-18：完成 GS-08 驗測一輪（4/4 pass）。
- 2026-02-18：改為系統自洽文件，不再使用跨文件引用描述。
