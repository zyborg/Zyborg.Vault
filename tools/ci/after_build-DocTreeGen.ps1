
## This script generates HTML API Documentation for Zyborg.Vault module

## Install and load the DTG module
Install-Module DocTreeGenerator -Scope CurrentUser -Force -AllowClobber -ErrorAction Stop
Import-Module DocTreeGenerator -Force

## We need to "FIX" the DTG module to handle modules with a DefaultCommandPrefix
## defined, until [this](https://github.com/msorens/DocTreeGenerator/pull/8) is
## merged in and the tool is republished.
$dtg = Get-Module DocTreeGenerator
$dtgBase = $dtg.ModuleBase
$dtgFile = "$dtgBase\Source\Convert-HelpToHtmlTree.ps1"
$dtgLines = [System.IO.File]::ReadAllLines($dtgFile)
Rename-Item  -Path $dtgFile -NewName "$($dtgFile).ORIG"
$dtgLines[434] = $dtgLines[434].Replace('$_', '$function')
[System.IO.File]::WriteAllLines($dtgFile, $dtgLines)


## Resolve input parameters and paths
$projectRoot = "." ## Root of Zyborg.Vault
$ciDir = "$projectRoot\tools\ci"

$templatePath = "$ciDir\DTG\bootstrap4_template.html"

$workDir = "$projectRoot\work"
mkdir $workDir -Force

$sourceDir = "$workDir\ZyborgSource"
$targetDir = "$workDir\api-docs"

mkdir $sourceDir -Force
mkdir $targetDir -Force

$ns    = "Zyborg"
$nsDir = "$workDir\ZyborgSource\$ns"

mkdir $nsDir -Force

## Copy over the latest build output to a folder structure required by DTG
##    SourceDir\NamespaceName\ModuleName\*
$buildConfig = $env:CONFIGURATION
if (-not $buildConfig) {
    $buildConfig = "Debug"
}
$buildOutDir = "$projectRoot\src\main\Zyborg.Vault.POSH\bin\$buildConfig\Zyborg.Vault"

Copy-Item -Path $buildOutDir -Destination $nsDir -Recurse
Copy-Item -Path "$ciDir\DTG\namespace_overview.html" -Destination $nsDir
Copy-Item -Path "$ciDir\DTG\module_overview.html" -Destination "$nsDir\Zyborg.Vault"

$docTitle  = "Vault API"
$copyright = "Copyright (C) Eugene Bekker.  All rights reserved."
$revDate   = (Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
if ($env:BUILD_NUMBER) {
    $revDate += " ($($env:BUILD_NUMBER))"
}

Convert-HelpToHtmlTree `
        -Namespaces   $ns `
        -SourceDir    $sourceDir `
        -TargetDir    $targetDir `
        -TemplateName $templatePath `
        -DocTitle     $docTitle `
        -Copyright    $copyright `
        -RevisionDate $revDate `
        -Verbose
