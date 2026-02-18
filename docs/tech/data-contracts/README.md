# Tech - Data Contract Standard

## 設計原則
- 所有存檔必須具版本欄位。
- Enum 使用字串序列化。
- unknown field 可忽略，必填缺失需拒載或 fallback。

## 必守規則
- 資料結構變更必須同步 schema 與 migration。
- 不允許未版本化的破壞性欄位調整。
