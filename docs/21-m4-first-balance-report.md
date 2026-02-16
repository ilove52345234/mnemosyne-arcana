# 21 - M4 首輪平衡報告（2026-02-14）

## 1. 範圍

本報告覆蓋：

- `M4-01` 詞庫內容填充（T1/T2）
- `M4-02` 商店池權重與價格帶調整
- `M4-03` 盲注曲線體感檔位（Relaxed/Standard/Challenging）

## 2. 首輪結果摘要

### 2.1 詞庫基線

- `word_entries.v2.json`：100 筆（T1=50, T2=50）
- 元素分布：Life/Force/Mind/Matter/Abstract 各 20
- 詞性分布：N=37, V=23, A=20, D=20
- 驗證守門已啟用：
- Tier 下限（T1/T2）
- 詞性/元素覆蓋
- `difficulty` 與 `baseChips` 範圍

### 2.2 商店平衡

- 價格帶符合 SoT：
- Sense 4-8
- Material 3-6
- Affix 2-4
- Course 10
- Ante 分段權重生效：
- 1-2：Material/Affix 偏高
- 3-5：Material/Sense 偏高
- 6-8：完整池 + Course 低機率
- Boss 商店：Course 固定 2 選 1

### 2.3 盲注曲線

- `Standard` 保持 SoT 原始曲線，不破壞既有平衡基線。
- 新增體感檔位：
- `Relaxed`：降低前中期壓力
- `Challenging`：提高前中期壓力
- 測試已鎖定 `Relaxed < Standard < Challenging`。

## 3. 目前判定

- `M4` 已達首輪可交付標準。
- 系統可進入 Alpha Gate 驗收階段。

## 4. 風險與限制

1. 本環境仍受 Unity 授權限制，無法完整跑 batchmode EditMode 測試。
2. 詞庫已可玩，但語意難度仍需教學設計進一步審稿。
3. 契約生成目前為固定池抽樣，尚未完全落地 40/40/20 權重模型。

## 5. Alpha 前建議動作

1. 先執行 `A-01` 全流程回歸（Ante 1-8）。
2. 補 `A-02` 存檔/migration 壓測與異常回退驗證。
3. 在可授權環境完成 EditMode 測試管線並保留測試報告。
