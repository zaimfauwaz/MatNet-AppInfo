<#
.SYNOPSIS
  Removes "MatNet AppInfo" from the right-click menu. Only removes what Register-ContextMenu.ps1 added.
#>
$extensions = ".exe", ".dll", ".msi", ".msp", ".sys", ".ocx", ".rll"
$menuNames  = "MatNetAppInfo", "MatNetAppInfoAdmin"

foreach ($ext in $extensions) {
    foreach ($menuName in $menuNames) {
        $key = "HKCU:\Software\Classes\SystemFileAssociations\$ext\shell\$menuName"
        if (Test-Path $key) {
            Remove-Item -Path $key -Recurse -Force
            Write-Host "Removed $menuName from $ext"
        }
    }
}
