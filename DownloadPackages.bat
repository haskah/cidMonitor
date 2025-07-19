@echo off
echo CloudVoice CID Service - Package Download
echo ==========================================

echo NuGet paketleri indiriliyor...

REM Packages klasorunu olustur
if not exist "packages" mkdir packages
if not exist "packages\Newtonsoft.Json.13.0.3\lib\net45" mkdir packages\Newtonsoft.Json.13.0.3\lib\net45

echo.
echo Newtonsoft.Json 13.0.3 indiriliyor...

REM PowerShell ile Newtonsoft.Json indir
powershell -Command "& {[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; Invoke-WebRequest -Uri 'https://www.nuget.org/api/v2/package/Newtonsoft.Json/13.0.3' -OutFile 'packages\Newtonsoft.Json.13.0.3.nupkg'}"

if exist "packages\Newtonsoft.Json.13.0.3.nupkg" (
    echo Paket indirildi, aciliyor...
    
    REM NuPkg dosyasini zip olarak ac
    powershell -Command "Expand-Archive -Path 'packages\Newtonsoft.Json.13.0.3.nupkg' -DestinationPath 'packages\Newtonsoft.Json.13.0.3' -Force"
    
    REM .nupkg dosyasini sil
    del "packages\Newtonsoft.Json.13.0.3.nupkg"
    
    if exist "packages\Newtonsoft.Json.13.0.3\lib\net45\Newtonsoft.Json.dll" (
        echo.
        echo ================================
        echo PAKET INDIRME BASARILI!
        echo ================================
        echo.
        echo Newtonsoft.Json.dll hazir.
        echo Simdi BuildProject.bat calistirabilirsiniz.
        echo.
    ) else (
        echo HATA: DLL dosyasi bulunamadi!
    )
) else (
    echo HATA: Paket indirilemedi!
    echo.
    echo Manuel indirme:
    echo 1. https://www.nuget.org/packages/Newtonsoft.Json/13.0.3 adresine gidin
    echo 2. "Download package" tiklayin
    echo 3. .nupkg dosyasini bu klasore koyun
    echo 4. BuildProject.bat calistirin
)

echo.
pause 