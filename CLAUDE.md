# CLAUDE.md - 專案約束規範

> 本文件為 Mnemosyne Arcana 的執行約束。所有 AI/人類開發者皆必須遵守。

## 1. 文件先行（Mandatory Read Before Development）

在開始任何開發、重構、調參、或提案前，必須先閱讀以下文件：

1. `README.md`
2. `docs/00-project-vision.md`
3. `docs/01-game-design-core.md`
4. `docs/02-meta-progression.md`
5. `docs/03-technical-architecture.md`
6. `docs/04-data-contracts.md`
7. `docs/05-development-workflow.md`
8. `docs/06-onboarding-checklist.md`
9. `docs/09-nfr-and-quality-gates.md`
10. `docs/10-runtime-state-and-event-contracts.md`
11. `docs/15-balance-source-of-truth.md`
12. `docs/16-config-governance.md`
13. `docs/17-test-matrix.md`
14. `docs/18-api-and-domain-types.md`
15. `docs/PROJECT_EXECUTION_PLAN.md`
16. `docs/IMPLEMENTATION_STATUS.md`
17. `docs/SESSION_NOTES.md`
18. `docs/19-system-development-boundaries.md`

違反本條視為流程不合格，不得宣稱任務完成。

## 2. 任務完成後強制動作

每完成一個任務（最小可交付單位）必須依序完成：

1. 更新 `docs/IMPLEMENTATION_STATUS.md`（狀態改為 `Done` 或 `Blocked`）
2. 在 `docs/SESSION_NOTES.md` 新增交接記錄
3. 如有規格變更，同步更新對應規格文件（01~18）
4. 提交 commit（訊息需可追溯）
5. 推送到遠端（`git push`）

任何缺漏皆視為任務未完成。

## 3. 設計衝突禁止條款（Hard Boundary）

所有新功能、數值與互動規則，若與核心設計文件衝突，直接不採用。

核心依據（由高到低）：
1. `docs/01-game-design-core.md`
2. `docs/02-meta-progression.md`
3. `docs/03-technical-architecture.md`
4. `docs/04-data-contracts.md`
5. `docs/15-balance-source-of-truth.md`

若提案衝突，處理方式：
1. 標記為 `Design Conflict`
2. 記錄於 `docs/SESSION_NOTES.md`
3. 不進入實作
4. 如需改動核心設計，必須先提交 ADR，再更新核心文件後才可開發

## 4. 實作紀律

1. 嚴禁以程式碼硬改核心數值，數值必須以設定檔與 SoT 文件為準。
2. 嚴禁繞過測試矩陣與品質門檻宣稱完成。
3. 嚴禁未完成 migration 策略即改動存檔結構。

## 5. 驗收口徑

任務可驗收的必要條件：
- 功能可運作
- 測試通過
- 文件同步
- 已推送遠端

以上四項缺一不可。
