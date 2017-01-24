$release = Invoke-RestMethod https://api.github.com/repos/danroth27/DotNetToolbox/releases/latest
$assets = $release.assets
$toolboxAsset = $assets | Where-Object {$_.name -eq "dotnet-toolbox.zip"}
$zipUrl = $toolboxAsset[0].browser_download_url
$outFile=[System.IO.Path]::GetTempFileName()
Invoke-WebRequest -UseBasicParsing -uri $zipUrl -OutFile $outFile
Add-Type -assembly System.IO.Compression.FileSystem
$outDir = "$outFile-extracted"
Write-Host $outDir
[System.IO.Compression.ZipFile]::ExtractToDirectory($outFile, $outDir)
dotnet $([System.IO.Path]::Combine($outDir, "dotnet-toolbox.dll")) install dotnet-toolbox -v 1.0.0-*
if ($LASTEXITCODE -ne 0) {
    Write-Host "Could not install."
    exit 1
}
$path = [Environment]::GetEnvironmentVariable("PATH", "User")
if ($path -notlike "*$env:USERPROFILE\.toolbox;*") {
    $newPath = "$env:USERPROFILE\.toolbox;$path"
    [Environment]::SetEnvironmentVariable("PATH", $newPath, "User")
    $env:Path="$env:USERPROFILE\.toolbox;$env:Path"
}