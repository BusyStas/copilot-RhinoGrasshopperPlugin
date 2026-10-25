@echo off
setlocal EnableExtensions
rem ---------------------------------------------------------------------------
rem Builds Stas Copilot (Release) and collects the plugin in a downloads folder.
rem
rem Usage:   _build.bat [output-folder]
rem Default: the "downloads" folder next to this file.
rem
rem Result:  <output>\StasCopilot\<framework>\StasCopilot.rhp  (plus dependencies)
rem          <output>\StasCopilot.zip
rem ---------------------------------------------------------------------------

cd /d "%~dp0"

set "OUT=%~1"
if not defined OUT set "OUT=%~dp0downloads"

where dotnet >nul 2>&1
if errorlevel 1 goto :nosdk

echo Building Stas Copilot (Release)...
dotnet build "src\RhinoCopilot.Plugin\RhinoCopilot.Plugin.csproj" -c Release --nologo -v minimal
if errorlevel 1 goto :buildfailed

set "BIN=%~dp0src\RhinoCopilot.Plugin\bin\Release"
set "DEST=%OUT%\StasCopilot"
set "COLLECTED="

if exist "%DEST%" rmdir /s /q "%DEST%"
mkdir "%DEST%"
if errorlevel 1 goto :copyfailed

for %%F in (net7.0-windows net7.0 net48) do call :collect %%F
if not defined COLLECTED goto :nooutput

powershell -NoProfile -ExecutionPolicy Bypass -Command "Compress-Archive -Path '%DEST%\*' -DestinationPath '%OUT%\StasCopilot.zip' -Force"
if errorlevel 1 goto :zipfailed

echo.
echo Done.
echo   Folder: %DEST%
echo   Zip:    %OUT%\StasCopilot.zip
echo.
echo To use it in Rhino 8: run PlugInManager, click Install, and pick
echo   %DEST%\net7.0-windows\StasCopilot.rhp
exit /b 0

:collect
if not exist "%BIN%\%~1\StasCopilot.rhp" goto :eof
xcopy "%BIN%\%~1" "%DEST%\%~1\" /e /i /y /q >nul
set "COLLECTED=1"
goto :eof

:nosdk
echo The .NET SDK was not found. Run _install.bat first.
exit /b 1

:buildfailed
echo Build failed. See the messages above.
exit /b 1

:copyfailed
echo Could not create the output folder: %DEST%
exit /b 1

:nooutput
echo The build finished but no StasCopilot.rhp was found under %BIN%
exit /b 1

:zipfailed
echo Could not create the zip file. The folder %DEST% is still usable.
exit /b 1
