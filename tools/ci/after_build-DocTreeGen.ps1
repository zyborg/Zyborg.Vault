
## This script generates HTML API Documentation for Zyborg.Vault module

## Install and load the DTG module
Install-Module DocTreeGenerator -Scope CurrentUser -Force -AllowClobber -ErrorAction Stop

## We need to "FIX" the DTG module to handle modules with a DefaultCommandPrefix
## defined, until [this](https://github.com/msorens/DocTreeGenerator/pull/8) is
## merged in and the tool is republished.
$dtg = Get-Module DocTreeGenerator -ListAvailable
$dtgBase = $dtg.ModuleBase
$dtgFile = "$dtgBase\Source\Convert-HelpToHtmlTree.ps1"
$dtgLines = [System.IO.File]::ReadAllLines($dtgFile)
Rename-Item  -Path $dtgFile -NewName "$($dtgFile).ORIG"
$dtgLines[434] = $dtgLines[434].Replace('$_', '$function')
[System.IO.File]::WriteAllLines($dtgFile, $dtgLines)

## Now load the module after the fix
Import-Module DocTreeGenerator -Force


## Resolve input parameters and paths
$projectRoot = "$(Resolve-Path .)" ## Root of Zyborg.Vault
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

## If a switch is set, then try to publish the API Docs to GitHub
if ([int]$((Resolve-DnsName pubapidocs.vault.zyborg-ci.bkkr.us -Type TXT).Text)) {
    Write-Output "Detected Enabled Flag 'PUBAPIDOCS'"
    $gitHubToken = $env:GITHUB_TOKEN
    if (-not $gitHubToken) {
        Write-Warning "Failed to resolve GITHUB token; ABORTING"
    }
    else {
        
        ## Adapted from guidelines found here:  https://www.appveyor.com/docs/how-to/git-push/
        ######################################################################################

        ## First setup the local cred store with the GH Personal Access Token
        git config --global credential.helper store
        Add-Content "$env:USERPROFILE\.git-credentials" "https://$($env:GITHUB_TOKEN):x-oauth-basic@github.com`n"
        git config --global user.email "no-reply@appveyor.com"
        git config --global user.name "appveyor"

        ## Then clone, update, commit and push the docs changes
        $ghPagesDir = "$workDir\gh-pages-branch"
        $ghPagesApiDocsDir = "$ghPagesDir\docs\api"
        mkdir $ghPagesDir
        Set-Location $ghPagesDir

        git clone https://github.com/zyborg/Zyborg.Vault.git --branch gh-pages --single-branch .

        ## Delete then create and copy to make sure we get rid of anything obselete
        if (Test-Path $ghPagesApiDocsDir) {
            Remove-Item $ghPagesApiDocsDir -Recurse
        }
        mkdir $ghPagesApiDocsDir
        Copy-Item -Path $targetDir\* -Destination $ghPagesApiDocsDir -Recurse -Force

        git add .
        git commit -m "Publishing API Docs from AppVeyor from build $($env:APPVEYOR_BUILD_NUMBER)"
        git push
    }
}

## Make sure we return to the project root in case we had to change paths for Git
Set-Location $projectRoot
