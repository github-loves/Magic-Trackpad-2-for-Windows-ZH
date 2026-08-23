@echo off
REM Compile the Chinese control panel with the .NET Framework 4 in-box compiler (no SDK needed)
cd /d "%~dp0src"

C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /nologo /target:winexe /platform:anycpu /optimize+ ^
  /win32manifest:app.manifest /win32icon:app.ico ^
  /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Management.dll ^
  /out:"..\Magic Trackpad2 For Windows.exe" Program.cs Theme.cs Main.cs Main.Designer.cs Properties_AssemblyInfo.cs

if %errorlevel% equ 0 (
    echo.
    echo Build OK: Magic Trackpad2 For Windows.exe
) else (
    echo.
    echo Build FAILED.
    rem Close a running copy of the panel first if you see CS0016 (output file locked).
)
pause
