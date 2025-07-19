@echo off
echo CloudVoice CID Service Durdurma
echo ================================

REM Admin yetkisi kontrolu
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Yonetici yetkileri tespit edildi.
) else (
    echo HATA: Bu script yonetici yetkileri ile calistirilmalidir.
    echo Sag tikla ve "Yonetici olarak calistir" secin.
    pause
    exit /b 1
)

echo.
echo CloudVoice CID Service durduruluyor...

REM Servis durumunu kontrol et
sc query "CloudVoiceCidService" >nul 2>&1
if %errorLevel% == 0 (
    REM Servis var, durdurmaya calis
    sc stop "CloudVoiceCidService"
    
    if %errorLevel% == 0 (
        echo.
        echo ========================
        echo SERVIS DURDURULDU!
        echo ========================
        echo.
        echo CloudVoice CID Service basariyla durduruldu.
        echo.
        echo Servisi tekrar baslatmak icin StartService.bat kullanin.
        echo.
        echo Servis durumunu kontrol etmek icin:
        echo sc query CloudVoiceCidService
        echo.
    ) else (
        echo.
        echo UYARI: Servis durdurma komutu basarisiz!
        echo.
        echo Servis zaten durdurulmus olabilir.
        echo Servis durumunu kontrol edin:
        echo sc query CloudVoiceCidService
        echo.
    )
) else (
    echo.
    echo HATA: CloudVoice CID Service kurulu degil!
    echo.
    echo Once InstallService.bat ile servisi kurun.
    echo.
)

echo.
pause 