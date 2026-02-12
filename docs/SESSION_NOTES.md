# 開發交接記錄

> 規則：每次工作結束，必須新增一筆記錄。

---

## 範本

### 交接記錄（YYYY-MM-DD）- 主題

- 目標：
- 完成內容：
- 變更檔案：
- 驗證結果：
- 風險/阻塞：
- 下一步：

---

## 交接記錄（2026-02-12）- 新專案文件體系建立

- 目標：建立可交接、可開發的規格與架構文檔體系
- 完成內容：
  - 建立 `README` 與 `docs/00~18` 主規格
  - 補齊 SA/SD 缺口（NFR、Runtime 契約、Risk、Context、Usecase、Balance SoT、Config 治理、Test Matrix、API 型別）
  - 建立 scripts/schema 基礎（config 驗證與測試入口）
- 變更檔案：
  - `README.md`
  - `docs/00-project-vision.md` ~ `docs/18-api-and-domain-types.md`
  - `scripts/*`
  - `docs/schemas/*`
- 驗證結果：文件存在、連結與目錄結構可讀
- 風險/阻塞：尚未進入 Unity 可執行專案骨架（M0）
- 下一步：開始 M0-01 / M0-02

## 交接記錄（2026-02-12）- M0 專案骨架完成（01~04）

- 目標：完成 M0 首批四項任務，建立可接手的最小開發入口
- 完成內容：
  - 建立 Unity 最小專案骨架（`Packages/manifest.json`、`ProjectSettings/ProjectVersion.txt`、`Assets/MnemosyneArcana/*`）
  - 建立五大 Manager V2 stubs 與核心 Domain/Service 契約型別
  - 建立 `configs/word_entries.v2.json`、`configs/meta_progress.v2.json` 範例資料並串接既有 config 驗證腳本
  - 建立 EditMode 測試入口（asmdef + `ManagerStubTests`）
- 變更檔案：
  - `Packages/manifest.json`
  - `ProjectSettings/ProjectVersion.txt`
  - `Assets/MnemosyneArcana/Scripts/**`
  - `Assets/MnemosyneArcana/Tests/EditMode/**`
  - `configs/word_entries.v2.json`
  - `configs/meta_progress.v2.json`
  - `README.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
- 驗證結果：
  - `bash scripts/validate_configs.sh` 通過
- 風險/阻塞：
  - 尚未在本機實跑 Unity batch test（需本機 Unity binary 路徑）
- 下一步：
  - 進入 M1-01：牌型判定引擎（先完成 `HandType` 判定 deterministic 測試）

## 交接記錄（2026-02-12）- M1-01 牌型判定引擎完成

- 目標：完成十種牌型判定邏輯，建立 deterministic 測試基線
- 完成內容：
  - `ScoringManagerV2` 新增由高到低牌型判定：`GrammarFlush -> PoSFlush -> ElemFlush -> FullHouse -> GrammarChain -> ElemTriple -> PoSTriple -> PoSPair -> ElemPair -> Word`
  - 實作語序鏈規則：`A -> N -> V -> D`，允許跳階與同詞性連續，逆序判定為失敗
  - 補上基礎計分組裝（SoT 基礎籌碼/倍率 + modifiers）與 deterministic 分數輸出
  - 新增 EditMode 測試案例（語序鏈、滿堂、語序同族、deterministic）
- 變更檔案：
  - `Assets/MnemosyneArcana/Scripts/Core/Managers/ScoringManagerV2.cs`
  - `Assets/MnemosyneArcana/Tests/EditMode/ScoringHandTypeTests.cs`
  - `Assets/MnemosyneArcana/Tests/EditMode/ManagerStubTests.cs`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/PROJECT_EXECUTION_PLAN.md`
- 驗證結果：
  - `bash scripts/validate_configs.sh` 通過
  - Unity EditMode 測試在此環境受 Licensing IPC 阻塞（`LicenseClient` channel timeout）
- 風險/阻塞：
  - CI 或本機需可用 Unity 授權服務，否則無法完成 batchmode 測試
- 下一步：
  - M1-02：補齊牌型升級成長值（教材卡）與答錯懲罰整合（Learning/Scoring 邊界）
