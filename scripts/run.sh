#!/usr/bin/env bash
set -euo pipefail

URL="${ASHABRIDGE_URL:-http://127.0.0.1:5088}"

dotnet run --project src/AshaBridge.Api/AshaBridge.Api.csproj --urls "$URL"
