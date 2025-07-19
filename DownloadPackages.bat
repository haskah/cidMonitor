@echo off
echo CloudVoice CID Service - Package Download
echo ==========================================

echo NuGet paketleri indiriliyor...

REM Packages klasorunu olustur
if not exist "packages" mkdir packages
if not exist "packages\Newtonsoft.Json.13.0.3\lib\net45" mkdir packages\Newtonsoft.Json.13.0.3\lib\net45

echo.
echo Newtonsoft.Json 13.0.3 indiriliyor...

REM Eski PowerShell ile uyumlu WebClient kullan
powershell -Command "& {$client = New-Object System.Net.WebClient; $client.DownloadFile('https://www.nuget.org/api/v2/package/Newtonsoft.Json/13.0.3', 'packages\Newtonsoft.Json.13.0.3.nupkg')}"

if exist "packages\Newtonsoft.Json.13.0.3.nupkg" (
    echo Paket indirildi, aciliyor...
    
    REM NuPkg dosyasini zip olarak ac (eski PowerShell icin alternative)
    powershell -Command "& {Add-Type -AssemblyName System.IO.Compression.FileSystem; [System.IO.Compression.ZipFile]::ExtractToDirectory('packages\Newtonsoft.Json.13.0.3.nupkg', 'packages\Newtonsoft.Json.13.0.3')}"
    
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
        echo Alternatif klasorler kontrol ediliyor...
        if exist "packages\Newtonsoft.Json.13.0.3\lib\net40\Newtonsoft.Json.dll" (
            echo .NET 4.0 versiyonu bulundu, kopyalaniyor...
            if not exist "packages\Newtonsoft.Json.13.0.3\lib\net45" mkdir packages\Newtonsoft.Json.13.0.3\lib\net45
            copy "packages\Newtonsoft.Json.13.0.3\lib\net40\Newtonsoft.Json.dll" "packages\Newtonsoft.Json.13.0.3\lib\net45\"
            echo DLL kopyalandi!
        ) else (
            echo Hic uygun DLL bulunamadi!
        )
    )
) else (
    echo HATA: Paket indirilemedi!
    echo.
    echo ALTERNATIF COZUM 1: Curl kullan
    echo curl -L -o packages\Newtonsoft.Json.13.0.3.nupkg "https://www.nuget.org/api/v2/package/Newtonsoft.Json/13.0.3"
    echo.
    echo ALTERNATIF COZUM 2: Manuel indirme
    echo 1. https://www.nuget.org/packages/Newtonsoft.Json/13.0.3 adresine gidin
    echo 2. "Download package" tiklayin
    echo 3. .nupkg dosyasini packages klasorune koyun
    echo 4. Bu script'i tekrar calistirin
    echo.
    echo ALTERNATIF COZUM 3: NuGet CLI kullan (eger yuklu ise)
    echo nuget install Newtonsoft.Json -Version 13.0.3 -OutputDirectory packages
)

echo.
pause 