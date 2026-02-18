# Tech - Architecture Standard

## 設計原則
- 模組單一責任：Run / Scoring / Learning / Shop / Meta 分層。
- UI 只讀公開狀態，不直接改寫 domain 私有欄位。
- 流程靠狀態機，不靠隱式 side effect。

## 必守規則
- 任何跨模組協作先定義契約，再落實程式。
- 破壞性架構調整需先補風險與決策紀錄。
