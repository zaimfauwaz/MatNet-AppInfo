<#
.SYNOPSIS
  Removes "MatNet AppInfo" from the right-click menu. Only removes what Register-ContextMenu.ps1 added.
#>
$extensions = ".exe", ".dll", ".msi", ".msp", ".sys", ".ocx", ".rll"
$menuName   = "MatNetAppInfo"

foreach ($ext in $extensions) {
    $key = "HKCU:\Software\Classes\SystemFileAssociations\$ext\shell\$menuName"
    if (Test-Path $key) {
        Remove-Item -Path $key -Recurse -Force
        Write-Host "Removed from $ext"
    }
}
