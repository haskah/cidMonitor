@echo off
echo CloudVoice CID Service Kaldirma
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
echo CloudVoice CID Service kaldiriliyor...

REM Servis var mi kontrol et
sc query "CloudVoiceCidService" >nul 2>&1
if %errorLevel% == 0 (
    echo Servis bulundu, kaldirilacak...
    
    REM Servisi durdur
    echo Servis durduruluyor...
    sc stop "CloudVoiceCidService" >nul 2>&1
    timeout /t 3 /nobreak >nul
    
    REM Servisi kaldir
    echo Servis kaldiriliyor...
    %SystemRoot%\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /u "%~dp0CloudVoiceCidService.exe"
    
    if %errorLevel% == 0 (
        echo.
        echo ========================
        echo KALDIRMA BASARILI!
        echo ========================
        echo.
        echo CloudVoice CID Service basariyla kaldirildi.
        echo.
        echo Not: Log dosyalari ve konfigurasyon dosyalari
        echo otomatik olarak silinmez. Manuel olarak silebilirsiniz.
        echo.
    ) else (
        echo.
        echo HATA: Servis kaldirma basarisiz!
        echo Manuel olarak kaldirmaya calisiliyor...
        sc delete "CloudVoiceCidService"
        
        if %errorLevel% == 0 (
            echo Servis manuel olarak kaldirildi.
        ) else (
            echo Servis kaldirma tamamen basarisiz.
        )
    )
) else (
    echo CloudVoice CID Service zaten kurulu degil.
)

echo.
pause 