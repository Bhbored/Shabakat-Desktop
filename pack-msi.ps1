$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$tfm = "net10.0-windows10.0.19041.0"
$publishDir = Join-Path $root "bin\Release\$tfm\win-x64\publish"
$installer = Join-Path $root "Installer\Shabakat.Installer.wixproj"

Write-Host "Publishing Shabakat (Release, win-x64, self-contained)..."
dotnet publish (Join-Path $root "Shabakat.csproj") -c Release -r win-x64 --self-contained true
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$exe = Join-Path $publishDir "Shabakat.exe"
if (-not (Test-Path $exe)) {
    throw "Publish succeeded but Shabakat.exe was not found at $publishDir"
}

$publishedInstaller = Join-Path $publishDir "Installer"
if (Test-Path $publishedInstaller) {
    Remove-Item $publishedInstaller -Recurse -Force
}

Write-Host "Building MSI..."
dotnet build $installer -c Release "-p:PublishDir=$publishDir\"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$msi = Join-Path $root "Installer\bin\Release\Shabakat.msi"
Write-Host "Installer ready: $msi"
