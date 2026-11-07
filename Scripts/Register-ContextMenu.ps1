<#
.SYNOPSIS
  Adds "MatNet AppInfo" to the right-click menu of the supported file types.
.EXAMPLE
  .\Register-ContextMenu.ps1 -ExePath "C:\Tools\MatNet\AppInfoGUI.exe"
  .\Register-ContextMenu.ps1        # uses the Debug build next to this repository
.NOTES
  Per-user (HKCU): no administrator rights needed. Undo with Unregister-ContextMenu.ps1.
  On Windows 11 the entry is under "Show more options" (Shift+F10).
#>
param(
    [string]$ExePath = (Join-Path $PSScriptRoot "..\AppInfoGUI\bin\Debug\net10.0-windows\AppInfoGUI.exe")
)

$extensions = ".exe", ".dll", ".msi", ".msp", ".sys", ".ocx", ".rll"
$menuName   = "MatNetAppInfo"
$menuText   = "MatNet AppInfo"

if (-not (Test-Path $ExePath)) {
    Write-Error "Cannot find $ExePath. Build the app first, or pass -ExePath."
    exit 1
}
$ExePath = (Resolve-Path $ExePath).Path

foreach ($ext in $extensions) {
    $key = "HKCU:\Software\Classes\SystemFileAssociations\$ext\shell\$menuName"
    New-Item -Path "$key\command" -Force | Out-Null
    Set-ItemProperty -Path $key -Name "(default)" -Value $menuText
    Set-ItemProperty -Path $key -Name "Icon" -Value "`"$ExePath`""
    Set-ItemProperty -Path "$key\command" -Name "(default)" -Value "`"$ExePath`" `"%1`""
    Write-Host "Added to $ext"
}
Write-Host "Done. Right-click a file and choose '$menuText'."
