param(
    [string] $Url = "http://127.0.0.1:5088"
)

$ErrorActionPreference = "Stop"

dotnet run --project src/AshaBridge.Api/AshaBridge.Api.csproj --urls $Url
