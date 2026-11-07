<#
.SYNOPSIS
  Downloads the list of known vulnerable drivers (LOLDrivers) for MatNet AppInfo.
.NOTES
  The app only reads this file. It never sends anything to the internet.
  Run it again from time to time, because new vulnerable drivers are found regularly.
  Saved to: %LOCALAPPDATA%\MatNet AppInfo\vulnerable-drivers.json
#>
$url    = "https://www.loldrivers.io/api/drivers.json"
$folder = Join-Path $env:LOCALAPPDATA "MatNet AppInfo"
$target = Join-Path $folder "vulnerable-drivers.json"
$temp   = "$target.download"

New-Item -ItemType Directory -Path $folder -Force | Out-Null

try {
    Invoke-WebRequest -Uri $url -OutFile $temp -UseBasicParsing

    # A quick sanity check before the new file replaces the old list. (ConvertFrom-Json is not used:
    # Windows PowerShell rejects this list because it has keys that differ only by letter case.)
    $file = Get-Item $temp
    $start = (Get-Content $temp -TotalCount 1).TrimStart()
    if ($file.Length -lt 100KB -or -not ($start.StartsWith("[") -or $start.StartsWith("{"))) {
        throw "the downloaded file does not look like the driver list ($($file.Length) bytes)"
    }
    Move-Item -Path $temp -Destination $target -Force
    Write-Host "Saved the list to $target"
}
catch {
    Remove-Item $temp -ErrorAction SilentlyContinue
    Write-Error "Could not update the list: $($_.Exception.Message). The old list (if any) is unchanged."
    exit 1
}
