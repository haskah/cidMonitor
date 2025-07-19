# CloudVoice CID Service

Cloud Voice sisteminden gelen aramaları COM4 portuna Caller ID formatında gönderen Windows servisi.

## Genel Bakış

Bu servis, mevcut **Samba CidMonitor** modülüne hiçbir değişiklik yapmadan Cloud Voice entegrasyonu sağlar. Servis, Cloud Voice'dan gelen aramaları yakalar ve bunları COM4 portuna Bell 202 FSK Caller ID protokolü formatında gönderir.

## Özellikler

- ✅ **COM4 Port Desteği**: CidMonitor'ın dinlediği COM4 portuna veri gönderir
- ✅ **Bell 202 FSK Protokolü**: Standart Caller ID protokolü implementasyonu
- ✅ **Webhook Desteği**: HTTP webhook ile Cloud Voice entegrasyonu
- ✅ **API Polling**: Cloud Voice API'sini periyodik olarak kontrol eder
- ✅ **Otomatik Başlatma**: Windows servisi olarak otomatik başlar
- ✅ **Detaylı Loglama**: Event Log ve dosya tabanlı loglama
- ✅ **Test Modu**: Debug ve test için kolay kurulum
- ✅ **Otomatik Test**: Servis başladığında test numarası gönderir

## Sistem Gereksinimleri

- Windows 7/8/10/11 veya Windows Server 2008 R2+
- .NET Framework 4.7.2 veya üzeri
- Yönetici yetkileri (servis kurulumu için)
- COM4 portu (mevcut CidMonitor tarafından dinleniyor)

## Kurulum

### 1. Projeyi Derle (ÖNEMLİ!)
C# projesi derlenmeden çalışmaz. Önce .exe haline getirilmesi gerekiyor:

#### Otomatik Derleme:
```cmd
# NuGet paketlerini indir
DownloadPackages.bat

# Projeyi derle  
BuildProject.bat
```

#### Visual Studio ile:
```cmd
# Visual Studio'da projeyi aç
# Build → Build Solution (Ctrl+Shift+B)
```

#### Manuel Derleme:
```cmd
# .NET Framework Developer Command Prompt
msbuild CloudVoiceCidService.csproj /p:Configuration=Release
```

### 2. Servisi Kur
```cmd
# Derleme sonrası bin\Release klasöründe
cd bin\Release
InstallService.bat
```

### 3. Manuel Kurulum
```cmd
# .NET Framework InstallUtil kullanarak
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe CloudVoiceCidService.exe

# Servisi başlat
sc start CloudVoiceCidService
```

## Yapılandırma

### App.config Ayarları

```xml
<appSettings>
    <!-- Cloud Voice API -->
    <add key="CloudVoiceApiUrl" value="https://your-api.com/calls/latest" />
    <add key="CloudVoiceApiKey" value="your-api-key" />
    
    <!-- Webhook -->
    <add key="WebhookPort" value="8080" />
    
    <!-- COM Port -->
    <add key="ComPort" value="COM4" />
    <add key="BaudRate" value="1200" />
    
    <!-- Polling -->
    <add key="PollingInterval" value="3000" />
    
    <!-- Test Ayarları -->
    <add key="EnableTestMode" value="true" />
    <add key="TestPhoneNumber" value="+905551234567" />
</appSettings>
```

### Cloud Voice Entegrasyonu

#### Webhook Yöntemi (Önerilen)
Cloud Voice sisteminizi aşağıdaki endpoint'e webhook gönderecek şekilde yapılandırın:

```
URL: http://your-server:8080/cloudvoice/
Method: POST
Content-Type: application/json

Payload:
{
    "phoneNumber": "+905551234567",
    "caller": "+905551234567", 
    "from": "+905551234567"
}
```

#### API Polling Yöntemi
Cloud Voice API'nizi periyodik olarak kontrol eder:

```
GET https://your-cloud-voice-api.com/api/calls/latest
Response: 
{
    "phoneNumber": "+905551234567"
}
```

## Kullanım

### Servis Yönetimi
```cmd
# Başlat
StartService.bat

# Durdur  
StopService.bat

# Kaldır
UninstallService.bat

# Durum kontrol
sc query CloudVoiceCidService
```

### Test Modu
Debug için executable'ı direkt çalıştırabilirsiniz:

```cmd
CloudVoiceCidService.exe
```

### VS Code ile Test
Visual Studio Code kullanarak geliştirebilirsiniz:

```cmd
# VS Code'da proje klasörünü açın
code .

# Extension'ları yükleyin (otomatik önerilecek)
# Terminal'da (Ctrl + `)
DownloadPackages.bat
BuildProject.bat
bin\Debug\CloudVoiceCidService.exe

# Veya F5 ile debug mode
```

Detaylı VS Code kullanımı için: **VSCode-Guide.md** dosyasını inceleyin.

### Otomatik Test Özelliği
**SERVİS HER BAŞLADIĞINDA OTOMATİK TEST YAPILIR!**

- ✅ Servis başladığında hemen test numarası gönderilir
- ✅ App.config'den test numarası ve test modu kontrolü
- ✅ COM4 portuna Bell 202 FSK formatında test verisi gönderilir
- ✅ Log dosyasında detaylı test sonucu görünür

**Test sonucunu kontrol edin:**
- Samba'da popup çıktı mı?
- Event Log'da "TEST CALLER ID GÖNDERİLDİ" mesajı var mı?
- CidMonitor modülü test numarasını yakaladı mı?

**Test ayarları:**
```xml
<!-- Test modunu açık/kapalı yapma -->
<add key="EnableTestMode" value="true" />

<!-- Test telefon numarası değiştirme -->
<add key="TestPhoneNumber" value="+902121234567" />
```

### Log Kontrolü
```cmd
# Event Viewer
eventvwr.msc -> Windows Logs -> Application -> CloudVoiceCidService

# Log dosyası
type CloudVoiceCid.log
```

## Çalışma Mantığı

### 1. Veri Akışı
```
Cloud Voice → Webhook/API → CloudVoice CID Service → COM4 Port → CidMonitor → Samba
```

### 2. Caller ID Protokolü
Servis, Bell 202 FSK standardında Caller ID mesajları oluşturur:

```
Channel Seizure (30 bytes 0x55)
↓
Mark Signal (18 bytes 0xFF)  
↓
Message Frame:
  - DLE SOH (Start)
  - Message Type (0x80)
  - Data Length
  - Date/Time Parameter
  - Phone Number Parameter  
  - DLE ETX (End)
  - Checksum
```

### 3. Event Handling
- **Webhook**: HTTP POST isteklerini dinler
- **Polling**: API'yi periyodik kontrol eder  
- **COM Port**: Caller ID verilerini seri porta yazar
- **Logging**: Tüm işlemleri loglar

## Sorun Giderme

### Servis Başlamıyor
1. Event Log'u kontrol edin
2. COM4 portunu başka uygulama kullanıyor olabilir
3. Yönetici yetkileri var mı kontrol edin

### COM Port Hatası
```cmd
# Mevcut COM portlarını listele
mode

# COM4 kullanımını kontrol et
netstat -an | findstr :COM4
```

### Webhook Çalışmıyor
1. Windows Firewall'da 8080 portunu açın
2. `netstat -an | findstr :8080` ile port dinlemeyi kontrol edin
3. Cloud Voice IP'si erişebiliyor mu kontrol edin

### CidMonitor Alamıyor
1. CidMonitor COM4'ü dinliyor mu kontrol edin
2. Baud rate (1200) doğru mu kontrol edin  
3. Log'da Caller ID gönderim mesajları var mı kontrol edin

## API Referansı

### Webhook Endpoint
```http
POST /cloudvoice/
Content-Type: application/json

{
    "phoneNumber": "+905551234567"
}

Response: 200 OK
{
    "status": "success"
}
```

### URL Encoded Format
```http
POST /cloudvoice/
Content-Type: application/x-www-form-urlencoded

phone=+905551234567&caller=+905551234567
```

### Direkt Numara Gönderimi  
```http
POST /cloudvoice/
Content-Type: text/plain

+905551234567
```

## Güvenlik

- Servis LocalSystem hesabı ile çalışır
- Sadece localhost'tan webhook kabul eder
- COM port erişimi için sistem yetkileri gerekir
- Log dosyalarında hassas bilgi bulunmaz

## Sürüm Geçmişi

### v1.0.0
- İlk sürüm
- COM4 port desteği
- Webhook ve API polling
- Bell 202 FSK protokolü
- Windows Service implementasyonu

## Destek

Sorunlar için:
1. Event Log'u kontrol edin
2. CloudVoiceCid.log dosyasını inceleyin  
3. Test modunda çalıştırıp debug yapın

## Lisans

Bu proje dahili kullanım içindir. 