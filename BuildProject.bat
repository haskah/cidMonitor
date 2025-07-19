@echo off
echo CloudVoice CID Service - Build Script
echo ======================================

REM .NET Framework path'lerini belirle
set MSBUILD_2019="%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
set MSBUILD_2017="%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
set MSBUILD_FRAMEWORK="%ProgramFiles(x86)%\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
set MSBUILD_NET="%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"

echo Proje derleniyor...
echo.

REM MSBuild'i bul ve kullan
if exist %MSBUILD_NET% (
    echo Visual Studio 2022 MSBuild bulundu.
    %MSBUILD_NET% CloudVoiceCidService.csproj /p:Configuration=Release /p:Platform="Any CPU"
    goto :build_done
)

if exist %MSBUILD_2019% (
    echo Visual Studio 2019 MSBuild bulundu.
    %MSBUILD_2019% CloudVoiceCidService.csproj /p:Configuration=Release /p:Platform="Any CPU"
    goto :build_done
)

if exist %MSBUILD_FRAMEWORK% (
    echo Build Tools MSBuild bulundu.
    %MSBUILD_FRAMEWORK% CloudVoiceCidService.csproj /p:Configuration=Release /p:Platform="Any CPU"
    goto :build_done
)

if exist %MSBUILD_2017% (
    echo Visual Studio 2017 MSBuild bulundu.
    %MSBUILD_2017% CloudVoiceCidService.csproj /p:Configuration=Release /p:Platform="Any CPU"
    goto :build_done
)

REM Framework CSC ile derlemeyi dene
echo MSBuild bulunamadi, .NET Framework CSC deneniyor...
"%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" ^
    /target:exe ^
    /out:CloudVoiceCidService.exe ^
    /reference:System.dll ^
    /reference:System.Core.dll ^
    /reference:System.Configuration.dll ^
    /reference:System.ServiceProcess.dll ^
    /reference:System.Configuration.Install.dll ^
    /reference:System.Net.Http.dll ^
    /reference:packages\Newtonsoft.Json.13.0.3\lib\net45\Newtonsoft.Json.dll ^
    CloudVoiceCidService.cs Program.cs ServiceInstaller.cs Properties\AssemblyInfo.cs

if %errorLevel% == 0 (
    echo.
    echo Manuel CSC derleme basarili!
    goto :build_done
) else (
    goto :build_error
)

:build_done
if %errorLevel% == 0 (
    echo.
    echo ================================
    echo DERLEME BASARILI!
    echo ================================
    echo.
    echo Dosyalar:
    if exist "bin\Release\CloudVoiceCidService.exe" (
        echo - bin\Release\CloudVoiceCidService.exe
        echo - bin\Release\CloudVoiceCidService.exe.config
        copy /Y InstallService.bat bin\Release\ >nul
        copy /Y UninstallService.bat bin\Release\ >nul
        copy /Y StartService.bat bin\Release\ >nul
        copy /Y StopService.bat bin\Release\ >nul
        copy /Y README.md bin\Release\ >nul
        echo.
        echo Kurulum icin:
        echo cd bin\Release
        echo InstallService.bat
    ) else if exist "CloudVoiceCidService.exe" (
        echo - CloudVoiceCidService.exe (manual build)
        echo.
        echo Manuel kurulum icin InstallUtil kullanin.
    )
    echo.
) else (
    goto :build_error
)

goto :end

:build_error
echo.
echo ================================
echo DERLEME HATASI!
echo ================================
echo.
echo Cozum onerileri:
echo 1. Visual Studio yukleyin
echo 2. .NET Framework 4.7.2 Developer Pack yukleyin
echo 3. NuGet package'larini restore edin:
echo    nuget restore packages.config
echo.

:end
pause 