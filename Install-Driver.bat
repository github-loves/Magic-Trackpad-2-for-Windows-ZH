@echo off
chcp 65001 >nul
cd /d "%~dp0"

REM Magic Trackpad 2 driver installer: trust cert + install driver package

net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [错误] 请右键本文件，选择"以管理员身份运行"后再试。
    pause
    exit /b 1
)

echo [1/2] 正在导入驱动作者证书到受信任的根证书存储区......
certutil -addstore -f Root MagicTrackpad2ForWindows.cer
if %errorlevel% neq 0 (
    echo [错误] 证书导入失败。请确认 MagicTrackpad2ForWindows.cer 和本文件在同一个文件夹里。
    pause
    exit /b 1
)

echo.
echo [2/2] 正在安装驱动程序......
pnputil /add-driver AMD64\AmtPtpDevice.inf /install
set "DRVRC=%errorlevel%"
if "%DRVRC%"=="0" (
    echo 驱动安装成功。
) else if "%DRVRC%"=="259" (
    echo 检测到驱动已安装且为最新版本，无需重复安装。
) else (
    echo [错误] 驱动安装失败，错误码 %DRVRC%。请确认 AMD64 文件夹和本文件在同一目录里。
    pause
    exit /b 1
)

echo.
echo 全部完成！现在请通过蓝牙配对 Magic Trackpad 2，或用数据线直连。
echo 如果之前已经配对过但触摸板没反应：删除蓝牙配对记录，重新配对一次即可。
echo 配对成功后光标可能延迟几秒钟出现，属于正常现象。
echo 之后可以运行 Magic Trackpad2 For Windows.exe 进行个性化调整。
pause
