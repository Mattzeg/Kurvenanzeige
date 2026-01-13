# Publish Script für Self-Contained Deployment
# Erstellt eine portable Version mit .NET Runtime

$ErrorActionPreference = "Stop"

# Datum für Ordnername
$targetDate = Get-Date -Format "yyyy-MM-dd"
$featureName = "S7PlcMonitor"

# Zielordner gemäß CLAUDE.md
$targetBase = "C:\Users\matth\OneDrive\Dokumente\Portable Anwendungen"
$targetFolder = "Kurvenanzeige-$targetDate-$featureName"
$targetPath = Join-Path $targetBase $targetFolder

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Kurvenanzeige - Self-Contained Publish" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Target: $targetPath" -ForegroundColor Yellow
Write-Host ""

# Prüfen ob Zielordner existiert
if (Test-Path $targetPath) {
    Write-Host "WARNUNG: Zielordner existiert bereits!" -ForegroundColor Red
    $response = Read-Host "Überschreiben? (j/n)"
    if ($response -ne "j") {
        Write-Host "Abgebrochen." -ForegroundColor Red
        exit 1
    }
    Remove-Item $targetPath -Recurse -Force
}

# Sicherstellen dass Basisordner existiert
if (!(Test-Path $targetBase)) {
    New-Item -ItemType Directory -Path $targetBase -Force | Out-Null
}

Write-Host "Building project..." -ForegroundColor Green

# Projekt bauen
$projectPath = "src\Kurvenanzeige.Web\Kurvenanzeige.Web.csproj"
dotnet build $projectPath -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build fehlgeschlagen!" -ForegroundColor Red
    exit 1
}

Write-Host "Publishing..." -ForegroundColor Green

# Self-Contained Publish
dotnet publish $projectPath `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -o $targetPath `
    /p:PublishSingleFile=false `
    /p:PublishReadyToRun=true `
    /p:IncludeNativeLibrariesForSelfExtract=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish fehlgeschlagen!" -ForegroundColor Red
    exit 1
}

Write-Host "Copying deployment files..." -ForegroundColor Green

# Deployment-Dateien kopieren
Copy-Item "deployment\appsettings.Production.json" "$targetPath\appsettings.json" -Force
Copy-Item "deployment\Start-Kurvenanzeige.bat" "$targetPath\" -Force
Copy-Item "README.md" "$targetPath\" -Force

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Publish erfolgreich!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Deployment-Ordner: $targetPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "Nächste Schritte:" -ForegroundColor Yellow
Write-Host "1. Passen Sie appsettings.json an (PLC IP-Adresse)" -ForegroundColor White
Write-Host "2. Starten Sie mit Start-Kurvenanzeige.bat" -ForegroundColor White
Write-Host "3. Browser öffnet sich automatisch auf http://localhost:5000" -ForegroundColor White
Write-Host ""

# Öffne Zielordner
explorer $targetPath
