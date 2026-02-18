# Tech - Runtime Event Standard

## 設計原則
- 事件命名固定為 domain.action.phase。
- payload 需可版本化並具向後相容策略。

## 必守規則
- 禁止雙向同步事件循環。
- 事件與 state 更新順序需可測試與可重現。
