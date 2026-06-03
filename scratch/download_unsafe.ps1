$url = "https://www.nuget.org/api/v2/package/System.Runtime.CompilerServices.Unsafe/6.0.0"
$output = "c:\Users\sa463\OneDrive\Documents\cms_import\packages\System.Runtime.CompilerServices.Unsafe.6.0.0.zip"
$dest = "c:\Users\sa463\OneDrive\Documents\cms_import\packages\System.Runtime.CompilerServices.Unsafe.6.0.0"

Write-Host "Downloading $url to $output ..."
Invoke-WebRequest -Uri $url -OutFile $output

Write-Host "Extracting to $dest ..."
if (-not (Test-Path $dest)) {
    New-Item -ItemType Directory -Path $dest -Force
}
Expand-Archive -Path $output -DestinationPath $dest -Force

Write-Host "Cleaning up ZIP file ..."
Remove-Item $output

Write-Host "Done!"
