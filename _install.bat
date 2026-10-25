@echo off
setlocal EnableExtensions
cd /d "%~dp0"

rem ---------------------------------------------------------------------------
rem Checks prerequisites, builds Stas Copilot and collects it in a downloads
rem folder. Everything this script prints is also saved to _install.txt.
rem
rem Usage: _install.bat [output-folder]
rem ---------------------------------------------------------------------------

if defined STAS_INSTALL_LOGGING goto :run

rem Relaunch through PowerShell so the output is shown on screen and saved to _install.txt.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\run-logged.ps1" -Log "%~dp0_install.txt" -ScriptName "_install.bat" %*
exit /b %errorlevel%

:run
echo.
echo Stas Copilot - setup and build
echo ==============================
echo.
echo Running the plugin needs no extra .NET install: Rhino 8 installs its own .NET runtime.
echo Building it from source needs the .NET SDK, version 8 or newer.
echo.

where dotnet >nul 2>&1
if errorlevel 1 goto :nosdk

set "SDKMAJOR="
for /f "tokens=1 delims=." %%V in ('dotnet --version 2^>nul') do set "SDKMAJOR=%%V"
if not defined SDKMAJOR goto :nosdk
if %SDKMAJOR% LSS 8 goto :oldsdk

echo Found .NET SDK version:
dotnet --version
echo.
goto :build

:oldsdk
echo The installed .NET SDK is older than version 8.
goto :offerinstall

:nosdk
echo The .NET SDK was not found.

:offerinstall
where winget >nul 2>&1
if errorlevel 1 goto :manual
echo Install the .NET 8 SDK now with winget? [Y/N]
choice /c YN /n >nul
if errorlevel 2 goto :manual
winget install --id Microsoft.DotNet.SDK.8 -e --accept-source-agreements --accept-package-agreements
if errorlevel 1 goto :wingetfailed
echo.
echo The .NET SDK was installed. Close this window, open a new one, and run
echo _install.bat again so the new SDK is on the PATH.
goto :end

:manual
echo.
echo Install the .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0
echo then run _install.bat again.
goto :end

:wingetfailed
echo.
echo winget could not install the SDK. Install it manually from
echo https://dotnet.microsoft.com/download/dotnet/8.0 and run _install.bat again.
goto :end

:build
call "%~dp0_build.bat" %*
if errorlevel 1 goto :failed

set "SHOW=%~dp0downloads"
if not "%~1"=="" set "SHOW=%~1"

echo.
if exist "%ProgramFiles%\Rhino 8\System\Rhino.exe" echo Rhino 8 found.
if not exist "%ProgramFiles%\Rhino 8\System\Rhino.exe" echo Rhino 8 was not found in the default location. Install it from rhino3d.com to use the plugin.
if not defined ANTHROPIC_API_KEY echo Note: the ANTHROPIC_API_KEY environment variable is not set. See docs\DEVELOPMENT.md.
echo.
echo Next, install the plugin in Rhino 8:
echo   1. Start Rhino 8 and run the command PlugInManager.
echo   2. Click Install and select %SHOW%\StasCopilot\net7.0-windows\StasCopilot.rhp
echo      (use the net48 folder instead if you switched Rhino to .NET Framework with SetDotNetRuntime)
echo   3. Run the command StasCopilot to open the panel.
explorer "%SHOW%"
goto :end

:failed
echo.
echo The build failed. See the messages above.

:end
echo.
echo This output was saved to %~dp0_install.txt
echo Press any key to close this window.
pause >nul
