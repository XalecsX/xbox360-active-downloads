[CmdletBinding()]
param([string]$DependenciesDirectory = (Join-Path $PSScriptRoot 'lib'))
$ErrorActionPreference = 'Stop'
$DependenciesDirectory = [IO.Path]::GetFullPath($DependenciesDirectory)
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
if (-not (Test-Path -LiteralPath $compiler)) { throw 'The .NET Framework C# compiler was not found.' }
foreach ($name in @('JRPC.dll', 'xdevkit.dll')) {
    if (-not (Test-Path -LiteralPath (Join-Path $DependenciesDirectory $name))) {
        throw "Supply your own $name in $DependenciesDirectory."
    }
}
$bin = Join-Path $PSScriptRoot 'bin'
$dist = Join-Path $PSScriptRoot 'dist'
New-Item -ItemType Directory -Path $bin, $dist -Force | Out-Null
$response = Join-Path $bin 'build.rsp'
$arguments = @(
    '/nologo', '/target:winexe', '/platform:anycpu', '/optimize+',
    '/reference:System.Windows.Forms.dll', '/reference:System.Drawing.dll',
    ('/reference:"{0}"' -f (Join-Path $DependenciesDirectory 'JRPC.dll')),
    ('/reference:"{0}"' -f (Join-Path $DependenciesDirectory 'xdevkit.dll')),
    ('/out:"{0}"' -f (Join-Path $bin 'XboxActiveDownloads.exe')),
    ('"{0}"' -f (Join-Path $PSScriptRoot 'ActiveDownloads.cs'))
)
[IO.File]::WriteAllLines($response, $arguments, [Text.Encoding]::UTF8)
& $compiler "@$response"
if ($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE." }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.md') -Destination (Join-Path $bin 'README.md') -Force
foreach ($name in @('JRPC.dll', 'xdevkit.dll')) {
    Copy-Item -LiteralPath (Join-Path $DependenciesDirectory $name) -Destination (Join-Path $bin $name) -Force
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'THIRD-PARTY-NOTICES.md') -Destination $bin -Force
$archive = Join-Path $dist 'XboxActiveDownloads-v1.0.1.zip'
$releaseFiles = @('XboxActiveDownloads.exe', 'JRPC.dll', 'xdevkit.dll', 'README.md', 'THIRD-PARTY-NOTICES.md') |
    ForEach-Object { Join-Path $bin $_ }
Compress-Archive -LiteralPath $releaseFiles -DestinationPath $archive -Force
Write-Host "Built: $(Join-Path $bin 'XboxActiveDownloads.exe')"
Write-Host "Release: $archive"
