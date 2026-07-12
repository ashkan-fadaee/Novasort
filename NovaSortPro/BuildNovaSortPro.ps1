# BuildNovaSortPro.ps1
# Created by Korosh and Nova for NovaSort Pro

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "         NovaSort Pro - 1-Click Builder (win-x64)      " -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host ""

# 1. Check for .NET 9 SDK
Write-Host "Checking for .NET 9 SDK..." -ForegroundColor Yellow
$dotnetInstalled = $false
try {
    $dotnetVersion = dotnet --version
    if ($dotnetVersion -like "9.*") {
        Write-Host "Found .NET SDK Version: $dotnetVersion" -ForegroundColor Green
        $dotnetInstalled = $true
    } else {
        Write-Host "Found different .NET version: $dotnetVersion. Attempting to build anyway..." -ForegroundColor Gray
        $dotnetInstalled = $true
    }
} catch {
    Write-Host ".NET SDK is not installed on this system." -ForegroundColor Red
}

if (-not $dotnetInstalled) {
    Write-Host "Downloading and installing .NET 9 SDK for Windows x64..." -ForegroundColor Yellow
    $url = "https://download.visualstudio.microsoft.com/download/pr/20042456-bf8c-4f7a-8b1e-450f38b0be2f/f586cb53e5e4faea80373d582a88fa9c/dotnet-sdk-9.0.100-win-x64.exe"
    $installerPath = "$env:TEMP\dotnet-sdk-9.0.exe"
    
    Write-Host "Downloading installer from Microsoft..." -ForegroundColor Gray
    Invoke-WebRequest -Uri $url -OutFile $installerPath
    
    Write-Host "Launching installer. Please complete the installation window..." -ForegroundColor Yellow
    Start-Process -FilePath $installerPath -Wait
    
    # Re-check dotnet
    try {
        $dotnetVersion = dotnet --version
        Write-Host "Success! .NET SDK Installed: $dotnetVersion" -ForegroundColor Green
    } catch {
        Write-Host "ERROR: Please install .NET 9 SDK manually and re-run this script." -ForegroundColor Red
        Pause
        Exit
    }
}

# 2. Restore and Build
Write-Host "Restoring NuGet Packages..." -ForegroundColor Yellow
dotnet restore NovaSortPro.csproj

Write-Host "Compiling NovaSort Pro win-x64 executable..." -ForegroundColor Yellow
dotnet publish NovaSortPro.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================================" -ForegroundColor Green
    Write-Host "       COMPILATION SUCCESSFUL! PORTABLE EXE IS READY!    " -ForegroundColor Green
    Write-Host "========================================================" -ForegroundColor Green
    
    $outPath = Resolve-Path "bin\x64\Release\net9.0-windows10.0.19041.0\win-x64\publish"
    Write-Host "Your 64-bit portable app is saved at:" -ForegroundColor Cyan
    Write-Host $outPath -ForegroundColor White
    
    # Open the output directory
    Explorer.exe $outPath
} else {
    Write-Host "ERROR: Compilation failed. Please check the logs above." -ForegroundColor Red
}

Write-Host "Press any key to close..."
$null = [System.Console]::ReadKey($true)
