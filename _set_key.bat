@echo off
setlocal EnableExtensions
cd /d "%~dp0"

rem ---------------------------------------------------------------------------
rem Saves your Anthropic API key as the ANTHROPIC_API_KEY user environment
rem variable, which Stas Copilot reads. Run it once, then restart Rhino.
rem
rem 1. Replace PUT_CLAUDE_KEY_HERE below with your key (keep the quotes).
rem 2. Run _set_key.bat.
rem
rem Never commit your key. After you run this script with a real key in it, the
rem script tells git to stop tracking changes to this file, so "git add" and
rem "git commit" skip it. This script never prints the key and is not logged.
rem ---------------------------------------------------------------------------

set "KEY=PUT_CLAUDE_KEY_HERE"

if "%KEY%"=="PUT_CLAUDE_KEY_HERE" goto :nokey

setx ANTHROPIC_API_KEY "%KEY%" >nul
if errorlevel 1 goto :failed

rem Keep the key out of git: ignore local changes to this file.
git update-index --skip-worktree _set_key.bat >nul 2>&1

echo.
echo ANTHROPIC_API_KEY was saved for your Windows user account.
echo Close and reopen Rhino (and any open terminal or IDE) so it picks up the key.
goto :end

:nokey
echo.
echo No key set. Open _set_key.bat in a text editor, replace PUT_CLAUDE_KEY_HERE
echo with your Anthropic API key, save the file, and run it again.
goto :end

:failed
echo.
echo Could not save the environment variable.

:end
echo.
echo Press any key to close this window.
pause >nul
