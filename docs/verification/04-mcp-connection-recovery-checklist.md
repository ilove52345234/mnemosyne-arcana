# 04 - MCP Connection Recovery Checklist

## 1. 標準設定
- Unity MCP server 啟動參數（HTTP mode）：
  - `--http-url http://127.0.0.1:8080`
- Codex MCP URL：
  - `unityMCP -> http://127.0.0.1:8080`

## 2. 快速驗證
1. `codex mcp list` 確認 URL 為 `http://127.0.0.1:8080`
2. Unity Console 應看到 plugin registered 與 tools registered
3. 進入新 session 後先做一次最小呼叫（例如 `manage_editor telemetry_status`）

## 3. 超時規則
- 任一 MCP tool call 超過 `15 秒` 視為失敗
- 不進行長時間等待或多輪重試

## 4. 失敗恢復流程（必做）
1. Stop Play Mode
2. 重新啟動 Unity MCP HTTP server
3. 確認無舊 server 殘留（避免多個 uvx 進程）
4. 重開 Codex session
5. 重新執行快速驗證

## 5. 常見症狀與判讀
- 症狀：`POST /mcp 404`
  - 判讀：client 仍使用舊 endpoint 或 transport cache
- 症狀：MCP call 60s timeout
  - 判讀：session transport 卡死，應重開 session
- 症狀：Cannot start tests while in Play Mode
  - 判讀：先 `stop` 再跑 `run_tests`
