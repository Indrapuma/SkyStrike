#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
MODEL_PATH="$REPO_DIR/artifacts/rl/qtable.json"
DOTNET_EXE="/c/Program Files/dotnet/dotnet.exe"

if [[ ! -f "$MODEL_PATH" ]]; then
  echo "Saved Q-table not found: $MODEL_PATH"
  echo "Run training first so the game can produce artifacts/rl/qtable.json"
  exit 1
fi

cd "$REPO_DIR"
"$DOTNET_EXE" run --project ./GameCSharp/GameCSharp.csproj -- --inference