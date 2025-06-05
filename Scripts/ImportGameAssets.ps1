function CopyFiles([string]$sourceFolder, [string]$destFolder)
{
	$sourceFilePaths = [System.IO.Directory]::GetFiles($SourceFolder, "*", [System.IO.SearchOption]::AllDirectories)

	foreach ($sourceFilePath in $sourceFilePaths)
	{
		$relFilePath = $sourceFilePath.Substring($sourceFolder.Length).TrimStart('\')
		$destFilePath = Join-Path $destFolder $relFilePath
		$d = Split-Path $destFilePath -Parent

		if (-not (Test-Path $d))
		{
			New-Item -ItemType Directory -Path $d | Out-Null
		}

		Copy-Item -Path $sourceFilePath -Destination $destFilePath
	}
}

function LogError
{
	param([string]$log) Write-Host $log -ForegroundColor Red
}

function LogTitle
{
	param([string]$title) Write-Host "> $title"
}

function GetClientName
{
	param([string]$configFileName)
	return "gai_$([System.Environment]::UserName)_$([guid]::NewGuid().ToString("N").Substring(0,8))"
}

function GetConfigFileContent
{
	param ([string]$configFilePath, [string]$artSourceRootPath)
	if (-not (Test-Path $configFilePath)) { LogError "No config file found: $configFilePath"; return $null }

	$relativeDestinationPath = $null
	$depotPaths = @()
	$parsingDepotPaths = $false
	$validDepotPaths = $true

	foreach ($l in Get-Content $configFilePath) 
	{
		$l = $l.Trim()

		if ($l -eq "") { continue }
		if ($l -like "#*") { continue }
		if ($l -like "RelativeDestinationPath:*")
		{
			$relativeDestinationPath = $l.Substring("RelativeDestinationPath:".Length).Trim()
			$parsingDepotPaths = $false
		}
		elseif ($l -eq "DepotPaths:")
		{
			$parsingDepotPaths = $true
		}
		elseif ($parsingDepotPaths -and $l -ne "")
		{
			if ($l -notlike "$artSourceRootPath*") { $validDepotPaths = $false }
			elseif ($l -notlike "*/Output" -and $l -notlike "*/Output/" ) { $validDepotPaths = $false }
			elseif ($l -like "*/Output") { $depotPaths += "$l/" }
			else { $depotPaths += $l }
		}
	}

	if ($depotPaths.Count -eq 0) { LogError "Missing depot path in $configFilePath"; return $null }
	if (-not $validDepotPaths) { LogError "Invalid depot path(s) in $configFilePath"; return $null }

	return [PSCustomObject]@{ RelativeDestinationPath = $relativeDestinationPath; DepotPaths = $depotPaths }
}

function GetPerforceClientFilePath
{
	$e = & where.exe p4 2>$null

	if ($e) { return $e }
	else    { return $null }
}

function GetVariable 
{
	param([string]$p4, [string]$name)
	$l = & $p4 set | Where-Object { $_ -like "$name=*" } | Select-Object -First 1
	if ($l) { return ($l -split '=',2)[1] -replace '\s+\(.*\)$','' } 
	else    { return $null }
}

function Login
{
	param([string]$p4)
	$l = & $p4 login -s 2>&1

	if ($l -match "ticket expires in") { return $true }

	$s = Read-Host -Prompt "Enter $(GetVariable $p4 "P4USER") P4 password" -AsSecureString
	$p = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($s))
	$l = $p | & $p4 login 2>&1

	if   ($l -match "logged in") { return $true }
	else { LogError $l; return $false }
}

function CreateClientView
{
	param([string]$p4, [string]$clientName, [string]$rootPath, [string[]]$viewMappings)

	$a = @()
	$a += "Client: $clientName"
	$a += "Owner: $([System.Environment]::UserName)"
	$a += "Host: $([System.Environment]::MachineName)"
	$a += "Description:"
	$a += "`tCreated for importing art files."
	$a += "Options: allwrite noclobber compress unlocked modtime rmdir"
	$a += "SubmitOptions: revertunchanged"
	$a += "Root: $rootPath"
	$a += "View:"

	foreach ($mapping in $viewMappings) { $a += "`t$mapping"}

	$clientSpec = $a -join "`n"
	$l = $clientSpec | & $p4 client -i 2>&1

	if ($l -match "Client $clientName saved.") { return $true; }
	else { return $false; }
}

function DeleteClient
{
	param([string]$p4, [string]$clientName)
	$l = & p4 client -d $clientName 2>&1
	return $l -like "*deleted."
}

function SyncFiles
{
	param([string]$p4, [string]$clientName)
	$l = & $p4 -c $clientName sync 2>&1

	if ($l -match "File\(s\) up-to-date") { return $true }

	return ($l | Where-Object { $_ -match "#\d+ - (updating|added|deleted)" } | Measure-Object).Count -eq $l.Count
}

# ========================================================================================
# Main Task Execution
# ========================================================================================

$artSourceRootPath = "//Ignite-Australia/ArtSource/"
$excludes = @(".txt")

Clear-Host
Write-Host "***********************************************" -ForegroundColor Green
Write-Host "*  Game Assets Importer for IGT Apac Studios  *" -ForegroundColor Green
Write-Host "***********************************************" -ForegroundColor Green
Write-Host ""

$configFilePath = Join-Path $PSScriptRoot "ImportGameAssetsConfig.txt"
$content = GetConfigFileContent $configFilePath $artSourceRootPath

if ($null -eq $content) { exit -1 }

$destinationFolder = Join-Path (Resolve-Path -Path "$PSScriptRoot\..") $content.relativeDestinationPath

Write-Host "You are about to import art assets."
Write-Host "From the art source output folder(s) in Perforce:"
foreach ($dp in $content.DepotPaths) { Write-Host " - $dp" -ForegroundColor Yellow }
Write-Host "To:"
Write-Host " - $destinationFolder" -ForegroundColor Yellow
Write-Host ""

do 
{
	$response = Read-Host "Is it correct and would you like to proceed? (Y/N)"
} while ($response -notmatch '^[YyNn]$')

if ($response -notmatch '^[Yy]$')
{
	Write-Host "Cancelled because the user selected No"; return
}
Write-Host ""

LogTitle "Check Perforce connectivity"
$p4 = GetPerforceClientFilePath

if ($null -eq $p4)
{ 
	LogError "Unable to locate p4.exe."
	LogError " - If you haven't downloaded Perforce Client, Please download https://www.perforce.com/downloads then retry"
	LogError " - If you have already installed Perforce Client, please contact us. This appears to be a Perforce environment setup issue on your PC."
	exit -1 
}

if (-not (Login $p4)) { exit -1 }

LogTitle "Prepare for importing"
$clientName = GetClientName
$workingFolder = Join-Path $env:TEMP $clientName

try
{
	$viewMappings = @()
	foreach ($dp in $content.DepotPaths)
	{
		$gpfn = Split-Path -Path (Split-Path -Path $dp -Parent) -Leaf
		$viewMappings += """$dp..."" ""//$clientName/art/$gpfn/..."""
		foreach ($exclude in $excludes)
		{
			$viewMappings += """-$dp...$exclude"" ""//$clientName/art/$gpfn/...$exclude"""
		}
	}
	if (-not (CreateClientView $p4 $clientName $workingFolder $viewMappings))
	{
		LogError "Unable to create workspace ($clientName)"
		exit -1
	}

	LogTitle "Download assets from art source output folder(s) in Perforce"
	if (-not(SyncFiles $p4 $clientName)) { LogError "Unable to sync files"; exit -1 }

	LogTitle "Import assets"
	$artWorkingFolder = Join-Path $workingFolder "art"
	CopyFiles $artWorkingFolder $destinationFolder

	Write-Host ""
	Write-Host "Successfully imported."
}
finally
{
	DeleteClient $p4 $clientName | Out-Null
	if (Test-Path $workingFolder) { Remove-Item -Path $workingFolder -Recurse -Force }
}