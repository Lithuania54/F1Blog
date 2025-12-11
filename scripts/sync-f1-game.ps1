# Downloads the upstream Godot Web export of the F1 game into the ASP.NET app.
# Usage:
#   pwsh scripts/sync-f1-game.ps1
# Optional:
#   pwsh scripts/sync-f1-game.ps1 -BaseUrl https://kuba--.github.io/f1/ -Destination src/F1.Web/wwwroot/game

param(
    [string]$BaseUrl = "https://kuba--.github.io/f1/",
    [string]$Destination = "src/F1.Web/wwwroot/game"
)

$files = @(
    "index.js",
    "index.pck",
    "index.wasm",
    "index.icon.png",
    "index.apple-touch-icon.png"
)

if (-not (Test-Path -LiteralPath $Destination)) {
    New-Item -ItemType Directory -Force -Path $Destination | Out-Null
}

Write-Host "Syncing F1 Web export from $BaseUrl to $Destination"

foreach ($file in $files) {
    $uri = ($BaseUrl.TrimEnd('/')) + "/" + $file
    $target = Join-Path $Destination $file

    Write-Host ("  -> {0}" -f $file)
    try {
        Invoke-WebRequest -UseBasicParsing -Uri $uri -OutFile $target
    }
    catch {
        Write-Error "Failed downloading $uri. $_"
        exit 1
    }
}

Write-Host "Done."
