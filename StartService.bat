@echo off
echo CloudVoice CID Service Baslatma
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
echo CloudVoice CID Service baslatiliyor...

REM Servis durumunu kontrol et
sc query "CloudVoiceCidService" >nul 2>&1
if %errorLevel% == 0 (
    REM Servis var, baslatmaya calis
    sc start "CloudVoiceCidService"
    
    if %errorLevel% == 0 (
        echo.
        echo ========================
        echo SERVIS BASLATILDI!
        echo ========================
        echo.
        echo CloudVoice CID Service basariyla baslatildi.
        echo.
        echo Servis Bilgileri:
        echo - COM Port: COM4
        echo - Webhook: http://localhost:8080/cloudvoice/
        echo - Log dosyasi: CloudVoiceCid.log
        echo.
        echo Servis durumunu kontrol etmek icin:
        echo sc query CloudVoiceCidService
        echo.
    ) else (
        echo.
        echo HATA: Servis baslatma basarisiz!
        echo.
        echo Servis zaten calisiyor olabilir veya bir hata olustu.
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