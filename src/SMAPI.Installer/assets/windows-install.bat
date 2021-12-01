@echo off
setlocal enabledelayedexpansion

SET installerDir="%~dp0"

REM make sure we're not running within a zip folder
echo %installerDir% | findstr /C:"%TEMP%" 1>nul
if %ERRORLEVEL% EQU 0 (
    echo Oops! It looks like you're running the installer from inside a zip file. Make sure you unzip the download first.
    echo.
    pause
    exit
)

REM make sure .NET 5 is installed
SET hasNet5=1
WHERE dotnet /q
if !ERRORLEVEL! NEQ 0 (
    SET hasNet5=0
) else (
    dotnet --info | findstr /C:"Microsoft.WindowsDesktop.App 5." 1>nul
    if !ERRORLEVEL! NEQ 0 (
        SET hasNet5=0
    )
)
if "%hasNet5%" == "0" (
    echo Oops! You don't have the required .NET version installed.
    echo.
    echo To install it:
    echo    1. Go to https://dotnet.microsoft.com/download/dotnet/5.0/runtime

    if "%PROCESSOR_ARCHITECTURE%" == "ARM64" (
        echo    2. Under "Run desktop apps", click "Download Arm64".
    ) else (
        echo    2. Under "Run desktop apps", click "Download x64".
    )

    echo    3. Run the downloaded installer.
    echo    4. Restart your computer.
    echo.
    pause
    exit
)

REM make sure an antivirus hasn't deleted the installer DLL
if not exist %installerDir%"internal\windows\SMAPI.Installer.dll" (
    echo Oops! SMAPI is missing one of its files. Your antivirus might have deleted it.
    echo Missing file: %installerDir%internal\windows\SMAPI.Installer.dll
    echo.
    pause
    exit
)

REM start installer
dotnet internal\windows\SMAPI.Installer.dll

REM keep window open if it failed
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Oops! The SMAPI installer seems to have failed. The error details may be shown above.
    echo.
    pause
    exit
)
