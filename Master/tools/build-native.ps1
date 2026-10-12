<#
.SYNOPSIS
  Builds the native runtime components of Nucleus Co-op and puts them in NucleusCoopTool\bin\<Configuration>:
  Nucleus.Hook32/64.dll, Nucleus.SHook32/64.dll, Nucleus.IJx86/x64.exe and the ProtoInput binaries.
.DESCRIPTION
  1. Initializes the ProtoInput submodule.
  2. Makes ATL available (VS component, or a user-level copy in %LOCALAPPDATA%\NucleusBuildDeps, no admin rights).
  3. Builds ProtoInput.sln Release x86 + x64 with v143 (tools\ProtoInput.v143.props, the submodule is not modified).
  4. Builds Nucleus.Hook, Nucleus.SHook and Nucleus.Inject for each configuration, x86 + x64. Their post-build steps
     copy the outputs to NucleusCoopTool\bin\<Configuration>.
  5. Copies ProtoInput\src\ProtoInput\Release\*.dll/*.exe there (what the Nucleus.Coop post-build also does).
  6. Checks the files listed in StartChecks.CheckFilesIntegrity: present, not empty, PE architecture matching the name.
  7. Checks that the ProtoInput DLLs export every function ProtoInput.cs imports.
  See docs/webui/host.md, "Building native components".
.PARAMETER NoReferenceBuild
  Do not build Nucleus.Gaming (referenced by Nucleus.Inject): use the one already built by Nucleus.Coop.csproj.
  Useful when another build of the C# projects may be running.
#>
param(
  [string[]]$Configuration = @('Debug', 'Release'),
  [switch]$NoReferenceBuild
)
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$master = Split-Path $PSScriptRoot -Parent
$repo = Split-Path $master -Parent
$protoDir = Join-Path $repo 'Submodules\ProtoInput\src\ProtoInput'
$props = Join-Path $PSScriptRoot 'ProtoInput.v143.props'

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vs = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
if (-not $vs) { throw 'Visual Studio 2022 (Build Tools) with the C++ x86/x64 tools (v143) is required.' }
$msbuild = Join-Path $vs 'MSBuild\Current\Bin\MSBuild.exe'
$vcTools = (Get-Content (Join-Path $vs 'VC\Auxiliary\Build\Microsoft.VCToolsVersion.default.txt') -TotalCount 1).Trim()

function Get-Json([string]$Url) {
  $content = (Invoke-WebRequest -UseBasicParsing $Url).Content
  if ($content -is [byte[]]) { $content = [Text.Encoding]::UTF8.GetString($content) }
  $content.TrimStart([char]0xFEFF) | ConvertFrom-Json
}

function Invoke-MSBuild([string[]]$Arguments) {
  Write-Host "MSBuild $($Arguments -join ' ')" -ForegroundColor Cyan
  & $msbuild @Arguments -nologo -v:minimal -nr:false
  if ($LASTEXITCODE -ne 0) { throw "MSBuild failed with exit code $LASTEXITCODE" }
}

# 1. ProtoInput submodule
$ErrorActionPreference = 'Continue'  # git writes progress to stderr
git -C $repo -c core.longpaths=true submodule update --init Submodules/ProtoInput
$ErrorActionPreference = 'Stop'
if ($LASTEXITCODE -ne 0) { throw 'git submodule update failed' }

# 2. ATL (BlackBone includes atlbase.h / atlcomcli.h and links atls.lib)
$atlUser = Join-Path $env:LOCALAPPDATA "NucleusBuildDeps\atl\$vcTools\atlmfc"
if (Test-Path (Join-Path $vs "VC\Tools\MSVC\$vcTools\atlmfc\include\atlbase.h")) {
  Write-Host "ATL: Visual Studio component (MSVC $vcTools)"
} elseif (Test-Path (Join-Path $atlUser 'include\atlbase.h')) {
  Write-Host "ATL: user-level copy $atlUser"
} else {
  Write-Host "ATL: not installed, downloading the MSVC $vcTools ATL packages from the Visual Studio catalog to $atlUser"
  $channel = Get-Json 'https://aka.ms/vs/17/release/channel'
  $manifestUrl = (($channel.channelItems | Where-Object { $_.type -eq 'Manifest' }).payloads | Select-Object -First 1).url
  $catalog = Get-Json $manifestUrl
  $mm = ($vcTools -split '\.')[0..1] -join '\.'
  $packages = $catalog.packages | Where-Object { $_.id -match "^Microsoft\.VC\.$mm\.[\d.]+\.ATL\.(Headers|X86|X64)\.base$" }
  if (@($packages).Count -ne 3) { throw "ATL packages for MSVC $vcTools not found in the current catalog. Install the VS component instead (see host.md)." }
  $tmp = Join-Path $env:TEMP ("nucleus-atl-" + [guid]::NewGuid())
  foreach ($p in $packages) {
    $payload = $p.payloads | Select-Object -First 1
    $dir = Join-Path $tmp $p.id
    New-Item -ItemType Directory -Force $dir | Out-Null
    $zip = Join-Path $dir 'package.zip'
    Invoke-WebRequest -UseBasicParsing $payload.url -OutFile $zip
    if ((Get-FileHash $zip -Algorithm SHA256).Hash -ne $payload.sha256.ToUpper()) { throw "SHA-256 mismatch for $($p.id)" }
    Expand-Archive $zip (Join-Path $dir 'x')
    $src = Get-ChildItem (Join-Path $dir 'x\Contents\VC\Tools\MSVC') -Directory | Select-Object -First 1
    if ($src.Name -ne $vcTools) { Write-Warning "$($p.id) is for MSVC $($src.Name), installed MSVC is $vcTools" }
    New-Item -ItemType Directory -Force (Split-Path $atlUser) | Out-Null
    Copy-Item (Join-Path $src.FullName 'atlmfc') (Split-Path $atlUser) -Recurse -Force
  }
  Remove-Item $tmp -Recurse -Force
}

# 3. ProtoInput (OpenXinput links DEVOBJ.lib without a solution dependency: build DEVOBJ first)
foreach ($platform in 'x86', 'x64') {
  Invoke-MSBuild @((Join-Path $protoDir 'ProtoInput.sln'), '-t:Xinput\DEVOBJ;Build', '-p:Configuration=Release', "-p:Platform=$platform",
    '-p:PlatformToolset=v143', '-p:WindowsTargetPlatformVersion=10.0', '-p:BuildProjectReferences=false',
    "-p:ForceImportBeforeCppTargets=$props")
}

# 4. Nucleus.Hook / Nucleus.SHook / Nucleus.Inject (solution platform x86 -> Win32/x86, x64 -> x64)
foreach ($config in $Configuration) {
  foreach ($platform in 'x86', 'x64') {
    $buildArgs = @((Join-Path $master 'NucleusCoop.sln'), '-t:Nucleus_Hook;Nucleus_SHook;Nucleus_Inject', "-p:Configuration=$config", "-p:Platform=$platform")
    if ($NoReferenceBuild) { $buildArgs += '-p:BuildProjectReferences=false' }
    Invoke-MSBuild $buildArgs
  }
}

# 5. ProtoInput binaries -> bin\<Configuration>
foreach ($config in $Configuration) {
  $bin = Join-Path $master "NucleusCoopTool\bin\$config"
  New-Item -ItemType Directory -Force $bin | Out-Null
  Get-ChildItem (Join-Path $protoDir 'Release') -File | Where-Object { $_.Extension -in '.dll', '.exe' } | ForEach-Object {
    for ($i = 1; ; $i++) {
      try { Copy-Item $_.FullName $bin -Force; break }
      catch { if ($i -ge 10) { throw }; Start-Sleep -Seconds 3 }  # file locked by a running NucleusCoop.exe or game
    }
  }
}

# 6. Check the files required by StartChecks.CheckFilesIntegrity
function Get-PeMachine([string]$path) {
  $b = [IO.File]::ReadAllBytes($path)
  if ($b.Length -lt 64 -or $b[0] -ne 0x4D -or $b[1] -ne 0x5A) { return 'not PE' }
  $pe = [BitConverter]::ToInt32($b, 0x3C)
  switch ([BitConverter]::ToUInt16($b, $pe + 4)) { 0x14C { 'x86' } 0x8664 { 'x64' } default { 'other' } }
}
$startChecks = Get-Content -Raw (Join-Path $master 'NucleusCoopTool\Tools\StartChecks.cs')
$list = [regex]::Match($startChecks, 'string\[\] ncFiles = \{(.*?)\};', 'Singleline').Groups[1].Value
$required = [regex]::Matches($list, '"([^"]+)"') | ForEach-Object { $_.Groups[1].Value }
$problems = 0
foreach ($config in $Configuration) {
  $bin = Join-Path $master "NucleusCoopTool\bin\$config"
  foreach ($name in $required) {
    $file = Join-Path $bin $name
    $status = 'ok'
    if (-not (Test-Path $file)) { $status = 'missing (build Nucleus.Coop.csproj for the managed files)' }
    elseif ((Get-Item $file).Length -eq 0) { $status = 'EMPTY' }
    elseif ($name -match '(32|x86)\.(dll|exe)$' -and (Get-PeMachine $file) -ne 'x86') { $status = 'not x86' }
    elseif ($name -match '(64|x64)\.(dll|exe)$' -and (Get-PeMachine $file) -ne 'x64') { $status = 'not x64' }
    if ($status -ne 'ok') { $problems++; Write-Warning "bin\$config\$name : $status" }
  }
}

# 7. Every ProtoInput function imported by the C# must be exported: a stale submodule only fails at game launch
#    (EntryPointNotFoundException, e.g. SetTranslateXinputtoMKB with the 2021 submodule).
$dumpbin = Join-Path $vs "VC\Tools\MSVC\$vcTools\bin\Hostx64\x64\dumpbin.exe"
$protoCs = Get-Content -Raw (Join-Path $master 'NucleusGaming\Coop\ProtoInput\ProtoInput.cs')
$imports = [regex]::Matches($protoCs, 'DllImport\("(ProtoInput[^"]+)"[^\]]*\]\s*public\s+static\s+extern\s+\S+\s+(\w+)\s*\(') |
  ForEach-Object { [pscustomobject]@{ Dll = $_.Groups[1].Value; Name = $_.Groups[2].Value } }
if (@($imports).Count -eq 0) { $problems++; Write-Warning 'No ProtoInput DllImport found in ProtoInput.cs: update the regex in build-native.ps1' }
foreach ($config in $Configuration) {
  foreach ($group in ($imports | Group-Object Dll)) {
    $file = Join-Path $master "NucleusCoopTool\bin\$config\$($group.Name)"
    if (-not (Test-Path $file)) { continue }  # already reported above
    $exports = & $dumpbin /exports $file | ForEach-Object { if ($_ -match '^\s+\d+\s+[0-9A-F]+\s+[0-9A-F]{8}\s+(\w+)') { $Matches[1] } }
    $missing = @($group.Group.Name | Sort-Object -Unique | Where-Object { $_ -notin $exports })
    if ($missing.Count -gt 0) {
      $problems += $missing.Count
      Write-Warning "bin\$config\$($group.Name) does not export $($missing -join ', ') (ProtoInput submodule older than ProtoInput.cs?)"
    }
  }
}
if ($problems -gt 0) { throw "$problems file check(s) failed" }
Write-Host "Native components OK in bin\$($Configuration -join ', bin\')" -ForegroundColor Green
