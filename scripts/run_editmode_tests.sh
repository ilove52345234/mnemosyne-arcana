#!/usr/bin/env bash
set -euo pipefail

PROJECT_PATH="$(cd "$(dirname "$0")/.." && pwd)"
UNITY_PATH="${UNITY_PATH:-}"

if [[ -z "$UNITY_PATH" ]]; then
  echo "[ERROR] UNITY_PATH is not set."
  echo "Example: UNITY_PATH='/Applications/Unity/Hub/Editor/2022.3.62f1/Unity.app/Contents/MacOS/Unity' $0"
  exit 1
fi

"$UNITY_PATH" -batchmode -quit \
  -projectPath "$PROJECT_PATH" \
  -runTests \
  -testPlatform editmode \
  -testResults "$PROJECT_PATH/TestResults/editmode-results.xml"

echo "[OK] EditMode tests finished."
