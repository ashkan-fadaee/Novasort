@echo off
echo ========================================================
echo NovaSort Pro - Automatic MSBuild Compiler
echo Created by Korosh and Nova
echo ========================================================
echo.
echo Searching for Visual Studio MSBuild installation...
for /f "usebackq tokens=*" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -property installationPath`) do (
  set "VS_PATH=%%i"
)

if "%VS_PATH%"=="" (
  echo ERROR: Visual Studio 2022 or MSBuild was not found on this machine!
  echo Please make sure Visual Studio 2022 is installed with C# Desktop Development.
  pause
  exit /b 1
)

echo Visual Studio found at: %VS_PATH%
echo Setting up environment variables...
call "%VS_PATH%\Common7\Tools\VsDevCmd.bat"

echo.
echo Restoring NuGet packages...
dotnet restore NovaSortPro.csproj

echo.
echo Compiling NovaSort Pro in Release mode (win-x64)...
msbuild NovaSortPro.csproj /p:Configuration=Release /p:Platform=x64 /p:RuntimeIdentifier=win-x64 /p:PublishSingleFile=true /p:SelfContained=true

if %ERRORLEVEL% NEQ 0 (
  echo.
  echo ERROR: Compilation failed! Please inspect the logs above.
  pause
  exit /b %ERRORLEVEL%
)

echo.
echo ========================================================
echo COMPILATION SUCCESSFUL!
echo ========================================================
echo Your portable executable is located at:
echo NovaSortPro\bin\x64\Release\net9.0-windows10.0.19041.0\win-x64\publish\NovaSortPro.exe
echo ========================================================
pause
