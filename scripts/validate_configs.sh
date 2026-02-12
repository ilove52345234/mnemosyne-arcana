#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
python3 "$PROJECT_ROOT/scripts/validate_configs.py"
