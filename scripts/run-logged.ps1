# Runs a batch file from the repository root, showing its output on screen
# and saving all of it to a log file. Used by _install.bat.
#
# Usage: run-logged.ps1 -Log <log file> -ScriptName <batch file> [arguments for the batch file]

param(
    [Parameter(Mandatory = $true)][string]$Log,
    [Parameter(Mandatory = $true)][string]$ScriptName,
    [Parameter(ValueFromRemainingArguments = $true)][string[]]$ScriptArgs
)

$ErrorActionPreference = 'Continue'

# This file lives in <repo>\scripts, so the batch file is one folder up.
$root = Split-Path -Parent $PSScriptRoot
Set-Location -LiteralPath $root

$stamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
Set-Content -LiteralPath $Log -Value "Log started $stamp" -Encoding UTF8

# Tells the batch file that it is already being logged, so it does not relaunch itself.
$env:STAS_INSTALL_LOGGING = '1'

& cmd.exe /c $ScriptName @ScriptArgs 2>&1 | ForEach-Object {
    $line = $_.ToString()
    Write-Host $line
    Add-Content -LiteralPath $Log -Value $line -Encoding UTF8
}

exit $LASTEXITCODE
