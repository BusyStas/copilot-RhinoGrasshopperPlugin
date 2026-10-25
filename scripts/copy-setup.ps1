# Copies _setup.bat from the repository root into a folder next to the plugin,
# always resetting the key line to the PUT_CLAUDE_KEY_HERE stub so a real API key
# typed into the repository copy can never end up in the committed downloads folder.
#
# Usage: copy-setup.ps1 -Destination <folder>

param(
    [Parameter(Mandatory = $true)][string]$Destination
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$source = Join-Path $root '_setup.bat'

$text = [System.IO.File]::ReadAllText($source)
$text = [System.Text.RegularExpressions.Regex]::Replace($text, '(?m)^set "KEY=[^"\r\n]*"', 'set "KEY=PUT_CLAUDE_KEY_HERE"')

$target = Join-Path $Destination '_setup.bat'
[System.IO.File]::WriteAllText($target, $text, [System.Text.Encoding]::ASCII)
