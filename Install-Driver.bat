@echo off
REM Magic Trackpad 2 driver installer: trust cert + install driver package

net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Please right-click this file and choose "Run as administrator".
    pause
    exit /b 1
)

echo [1/2] Importing author certificate to Trusted Root...
certutil -addstore -f Root MagicTrackpad2ForWindows.cer
if %errorlevel% neq 0 (
    echo [ERROR] Failed to import certificate.
    pause
    exit /b 1
)

echo.
echo [2/2] Installing driver package (AMD64)...
pnputil /add-driver AMD64\AmtPtpDevice.inf /install

echo.
echo Done. Now pair the Magic Trackpad 2 via Bluetooth (or plug in USB cable).
echo If it was already paired but does not work, remove the pairing and pair again.
pause
