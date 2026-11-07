<#
.SYNOPSIS
  Adds "MatNet AppInfo" to the right-click menu of the supported file types.
.EXAMPLE
  .\Register-ContextMenu.ps1 -ExePath "C:\Tools\MatNet\AppInfoGUI.exe"
  .\Register-ContextMenu.ps1        # uses the Debug build next to this repository
.NOTES
  Per-user (HKCU): no administrator rights needed. Undo with Unregister-ContextMenu.ps1.
  Two entries are added:
    "MatNet AppInfo"                 normal rights
    "MatNet AppInfo (administrator)" asks for administrator rights. Only shown while Shift is held.
  On Windows 11 the entries are under "Show more options" (or Shift+right-click).
#>
param(
    [string]$ExePath = (Join-Path $PSScriptRoot "..\AppInfoGUI\bin\Debug\net10.0-windows\AppInfoGUI.exe")
)

$extensions = ".exe", ".dll", ".msi", ".msp", ".sys", ".ocx", ".rll"
$menuName   = "MatNetAppInfo"
$menuText   = "MatNet AppInfo"
$adminName  = "MatNetAppInfoAdmin"
$adminText  = "MatNet AppInfo (administrator)"

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

    # The administrator entry: Extended means "only when Shift is held", HasLUAShield adds the shield icon
    $adminKey = "HKCU:\Software\Classes\SystemFileAssociations\$ext\shell\$adminName"
    New-Item -Path "$adminKey\command" -Force | Out-Null
    Set-ItemProperty -Path $adminKey -Name "(default)" -Value $adminText
    Set-ItemProperty -Path $adminKey -Name "Icon" -Value "`"$ExePath`""
    New-ItemProperty -Path $adminKey -Name "Extended" -Value "" -PropertyType String -Force | Out-Null
    New-ItemProperty -Path $adminKey -Name "HasLUAShield" -Value "" -PropertyType String -Force | Out-Null
    Set-ItemProperty -Path "$adminKey\command" -Name "(default)" -Value "`"$ExePath`" `"%1`" --admin"

    Write-Host "Added to $ext"
}
Write-Host "Done. Right-click a file and choose '$menuText'. Hold Shift for '$adminText'."
