# Script para instalar .NET 8 SDK
# Ejecutar como Administrador

Write-Host "Instalando .NET 8 SDK..." -ForegroundColor Green

# Opción 1: Usando winget
if (Get-Command winget -ErrorAction SilentlyContinue) {
    winget install Microsoft.DotNet.SDK.8
    Write-Host "SDK instalado via winget" -ForegroundColor Green
} else {
    Write-Host "winget no está disponible. Descargando instalador..." -ForegroundColor Yellow
    
    # Opción 2: Descargar e instalar manualmente
    $url = "https://download.visualstudio.microsoft.com/download/pr/93961dfa-7670-4ba8-9996-3a9c521e355d/0865358926a1e7c6f835f4ba5379f5f4/dotnet-sdk-8.0.100-win-x64.exe"
    $output = "$env:TEMP\dotnet-sdk-8.0.100-win-x64.exe"
    
    Write-Host "Descargando desde: $url" -ForegroundColor Cyan
    Invoke-WebRequest -Uri $url -OutFile $output
    
    Write-Host "Iniciando instalación..." -ForegroundColor Green
    Start-Process -FilePath $output -ArgumentList "/quiet" -Wait
    
    Remove-Item $output
}

Write-Host "Instalación completada. Verificando..." -ForegroundColor Green
dotnet --list-sdks

Write-Host "`n¡Listo! Ahora puedes compilar tu proyecto con .NET 8" -ForegroundColor Green
