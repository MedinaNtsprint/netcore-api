<#
Small development helper script for Windows PowerShell.
Usage:
  - Open PowerShell in the repository root and run:
      ./dev.ps1 -Help

Functions:
  - Invoke-RestoreBuild : restore and build the solution
  - Invoke-RunTests     : run the test suite
  - Invoke-RunApi       : run the API (from APICore.API folder)
  - New-TestJwtKey      : generate a 32-byte random key and print as Base64 and UTF-8 (32 chars likely)
#>

param(
    [switch]$Help,
    [switch]$RestoreBuild,
    [switch]$RunTests,
    [switch]$RunApi,
    [switch]$NewTestJwtKey
)

function Invoke-RestoreBuild {
    Write-Host "Running dotnet restore and build..." -ForegroundColor Cyan
    dotnet restore
    dotnet build "APICore.sln" -c Debug
}

function Invoke-RunTests {
    Write-Host "Running dotnet test..." -ForegroundColor Cyan
    dotnet test "APICore.sln" -c Debug
}

function Invoke-RunApi {
    Write-Host "Running APICore.API... (press Ctrl+C to stop)" -ForegroundColor Cyan
    Push-Location "APICore.API"
    dotnet run
    Pop-Location
}

function New-TestJwtKey {
    # Generate a 32-byte random key and show Base64 and UTF-8 form
    $bytes = New-Object byte[] 32
    [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
    $base64 = [System.Convert]::ToBase64String($bytes)
    $utf8 = [System.Text.Encoding]::UTF8.GetString($bytes)

    Write-Host "Base64 (32 bytes):" -ForegroundColor Green
    Write-Host $base64
    Write-Host "\nUTF8 (may include non-printable chars) - safe option is the Base64 value:" -ForegroundColor Yellow
    Write-Host $utf8
}

if ($Help) {
    Write-Host "dev.ps1 - helper script" -ForegroundColor Cyan
    Write-Host "  -RestoreBuild    Restore and build the solution"
    Write-Host "  -RunTests        Run the test suite"
    Write-Host "  -RunApi          Run the API project (APICore.API)"
    Write-Host "  -NewTestJwtKey   Generate a 32-byte Base64 JWT key (recommended for HS256 testing)"
    return
}

if ($RestoreBuild) { Invoke-RestoreBuild }
if ($RunTests) { Invoke-RunTests }
if ($RunApi) { Invoke-RunApi }
if ($NewTestJwtKey) { New-TestJwtKey }

if (-not ($RestoreBuild -or $RunTests -or $RunApi -or $NewTestJwtKey -or $Help)) {
    Write-Host "No parameters provided. Run ./dev.ps1 -Help for usage." -ForegroundColor Yellow
}
