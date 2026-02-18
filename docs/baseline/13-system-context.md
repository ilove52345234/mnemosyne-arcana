# 13 - 系統邊界與上下文圖

## 1. 系統目的

定義 `Mnemosyne Arcana` 的邊界、外部依賴與資料責任，避免開發時把工具、內容與執行期邏輯混在一起。

## 2. 系統邊界

系統內（In Scope）：
- Unity Client（Run/Meta/UI）
- 本地存檔（Meta、WordProgress、RunSnapshot）
- JSON 設定檔（玩法、商店、詞庫、課程樹）
- 測試與驗證工具（本地/CI）

系統外（Out of Scope for MVP）：
- 線上帳號系統
- 雲端同步
- 伺服器對戰

## 3. 上下文圖（文字版）

```text
[Designer]
   | 編輯平衡與內容
   v
[Config JSON + Schema]
   | 載入
   v
[Unity Game Client]
   | 讀寫
   v
[Local Save Files]

[Developer] <-> [Unity Game Client + Tests + CI]
[Player]    <-> [Unity Game Client]
```

## 4. 外部依賴

1. Unity LTS + C# Runtime
2. GitHub（版本控管）
3. CI Runner（測試/驗證）

## 5. 邊界規則

1. Domain 規則不可直接依賴 UI 物件。
2. UI 只讀公開狀態，不直接改寫 domain 內部欄位。
3. 設定檔是玩法真相來源，硬編碼數值視為違規。
