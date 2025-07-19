@echo off
echo CloudVoice CID Service Kurulumu
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
echo CloudVoice CID Service kuruluyor...

REM Onceki kurulum varsa kaldir
sc query "CloudVoiceCidService" >nul 2>&1
if %errorLevel% == 0 (
    echo Onceki kurulum bulundu, kaldirilacak...
    sc stop "CloudVoiceCidService" >nul 2>&1
    sc delete "CloudVoiceCidService" >nul 2>&1
    timeout /t 2 /nobreak >nul
)

REM Servisi kur
echo Servis kuruluyor...
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe "%~dp0CloudVoiceCidService.exe"

if %errorLevel% == 0 (
    echo.
    echo ========================
    echo KURULUM BASARILI!
    echo ========================
    echo.
    echo CloudVoice CID Service basariyla kuruldu.
    echo.
    echo Servis otomatik olarak baslatildi ve sistem yeniden basladiginda
    echo otomatik olarak calisacak.
    echo.
    echo Onemli Bilgiler:
    echo - Servis COM4 portunu dinliyor
    echo - Webhook: http://localhost:8080/cloudvoice/
    echo - Log dosyasi: CloudVoiceCid.log
    echo.
    echo Servis Yonetimi:
    echo - Baslat: StartService.bat
    echo - Durdur: StopService.bat  
    echo - Kaldir: UninstallService.bat
    echo.
) else (
    echo.
    echo HATA: Servis kurulumu basarisiz!
    echo Lutfen hata mesajlarini kontrol edin.
)

echo.
pause 