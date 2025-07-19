# VS Code ile CloudVoice CID Service Test Kılavuzu

Bu kılavuz Visual Studio Code ile .NET Framework projesini nasıl test edeceğinizi açıklar.

## 🚀 Hızlı Başlangıç

### 1. VS Code Kurulumu
```cmd
# VS Code'u indirin: https://code.visualstudio.com/
# CloudVoiceCidService klasörünü VS Code'da açın
code .
```

### 2. Gerekli Extension'ları Yükleyin
VS Code açıldığında otomatik öneri gelecek, şunları yükleyin:
- ✅ **C# for Visual Studio Code** (ms-dotnettools.csharp)
- ✅ **C# Dev Kit** (ms-dotnettools.csdevkit) 
- ✅ **.NET Install Tool** (ms-dotnettools.vscode-dotnet-runtime)
- ✅ **XML** (redhat.vscode-xml)
- ✅ **Code Runner** (formulahendry.code-runner)

### 3. .NET Framework SDK Kontrol
```cmd
# Developer Command Prompt for Visual Studio açın
msbuild -version

# Yoksa indirin: https://dotnet.microsoft.com/download/visual-studio-sdks
```

## ⚡ MSBuild Sorunu Hızlı Çözüm

**Path to shell executable "msbuild" does not exist** hatası alıyorsanız:

### 🚀 En Kolay Çözüm
```cmd
# VS Code Terminal'da (Ctrl + `)
# Bu batch file otomatik MSBuild bulur
BuildProject.bat
```

### 🔧 Task ile Çözüm
```
Ctrl+Shift+P → "Tasks: Run Task" → aşağıdakilerden birini dene:
✅ "build-project" (En güvenilir - batch kullanır)
✅ "build-vs2022" (VS 2022 varsa)  
✅ "build-vs2019-community" (VS 2019 Community varsa)
✅ "build-dotnet-framework" (.NET Framework MSBuild)
```

## 🔧 VS Code Test Yöntemleri

### Yöntem 1: Integrated Terminal (En Kolay)
```cmd
# VS Code'da Terminal açın (Ctrl + `)
# 1. Paketleri indir
DownloadPackages.bat

# 2. Projeyi derle  
BuildProject.bat

# 3. Test modunda çalıştır
bin\Debug\CloudVoiceCidService.exe
```

### Yöntem 2: VS Code Tasks (Ctrl+Shift+P)
```
1. Ctrl+Shift+P → "Tasks: Run Task"
2. "download-packages" seç → Enter
3. "build-project" seç → Enter  
4. Terminal'da: bin\Debug\CloudVoiceCidService.exe
```

### Yöntem 3: MSBuild ile (Gelişmiş)
```cmd
# VS Code Terminal'da
msbuild CloudVoiceCidService.csproj /p:Configuration=Debug
bin\Debug\CloudVoiceCidService.exe
```

### Yöntem 4: Debug Mode (F5)
```
1. F5 veya Run → Start Debugging
2. "Debug CloudVoice Service" seç
3. External terminal'da çalışır
```

## 🐛 Debug Özellikleri

### Breakpoint Koyma
```csharp
// CloudVoiceCidService.cs dosyasında satır numarasının yanına tıklayın
private void SendTestCallerID()
{
    // Buraya breakpoint koyabilirsiniz
    WriteLog("Test başlıyor...");
}
```

### Variables ve Watch
- **Variables panel**: F5 ile debug başlattığınızda değişkenleri görün
- **Watch panel**: Özel expression'ları izleyin
- **Call Stack**: Method çağrı yığınını görün

### Debug Console
```csharp
// Debug sırasında console'da komut çalıştırabilirsiniz
System.Console.WriteLine("Test mesajı");
```

## 📝 IntelliSense ve Code Completion

### Otomatik Tamamlama
- **Ctrl+Space**: Kod tamamlama
- **F12**: Definition'a git  
- **Shift+F12**: References'ları göster
- **Ctrl+.**: Quick fix önerileri

### Error Detection
- ❌ **Kırmızı dalga**: Syntax hatası
- ⚠️ **Sarı dalga**: Warning
- 💡 **Ampul ikonu**: Suggestion

## 🔍 Test Sonuçları

### Console Output Kontrol
```
CloudVoice CID Service - Debug Mode
==========================================
SERVİS BAŞLADI - TEST CALLER ID GÖNDERİLİYOR
==========================================
Test telefon numarası: +905551234567
...
```

### Log Dosyası İzleme
```cmd
# VS Code'da log dosyasını açın
CloudVoiceCid.log

# Veya terminal'da real-time izleme
Get-Content CloudVoiceCid.log -Wait -Tail 10
```

### COM Port Test
```cmd
# PowerShell'de COM port durumu
Get-WmiObject -Class Win32_SerialPort | Format-Table Name,DeviceID,Description

# Terminal'da port kullanımı
netstat -an | findstr COM
```

## ⚡ Hızlı Komutlar

| Kısayol | Açıklama |
|---------|----------|
| `Ctrl+Shift+P` | Command Palette |
| `Ctrl+`` ` | Terminal aç/kapat |
| `F5` | Debug başlat |
| `Shift+F5` | Debug durdur |
| `F9` | Breakpoint ekle/kaldır |
| `F10` | Step over |
| `F11` | Step into |
| `Ctrl+Shift+B` | Build task çalıştır |

## 🛠️ Sorun Giderme

### OmniSharp Çalışmıyor
```cmd
# VS Code'da Ctrl+Shift+P
# "OmniSharp: Restart OmniSharp" çalıştır
```

### MSBuild Bulunamadı
```cmd
# Çözüm 1: Developer Command Prompt'tan VS Code başlat
"%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\VsDevCmd.bat"
code .

# Çözüm 2: Alternatif task kullan
Ctrl+Shift+P → "Tasks: Run Task" → "build-project" seç (BuildProject.bat kullanır)

# Çözüm 3: Farklı VS sürümleri dene
Ctrl+Shift+P → "Tasks: Run Task" → aşağıdakilerden birini seç:
- build-vs2022 (VS 2022 Enterprise)
- build-vs2019-community (VS 2019 Community)  
- build-dotnet-framework (.NET Framework MSBuild)
```

### Extension Sorunları
```cmd
# VS Code'u admin olarak çalıştırın
# Extensions panelinde C# extension'ı reload edin
```

### COM Port Hatası
```cmd
# COM4 portunu başka uygulama kullanıyor olabilir
# Device Manager'da COM portlarını kontrol edin
```

## 🎯 Test Senaryoları

### 1. Temel Test
- Servis başlatılır
- Test numarası gönderilir
- Console'da başarı mesajı görülür

### 2. COM Port Test  
- SerialPort.Open() başarılı olur
- Test verisi COM4'e yazılır
- Hata logları kontrol edilir

### 3. Configuration Test
- App.config okunur
- Test ayarları uygulanır
- Farklı numara test edilir

### 4. Error Handling Test
- COM port kapalıyken test
- Yanlış config ile test
- Exception handling kontrol

## 📋 VS Code Avantajları

✅ **Hafif ve hızlı**
✅ **Integrated terminal**
✅ **Git entegrasyonu**
✅ **IntelliSense desteği**
✅ **Debug özellikleri**
✅ **Extension ekosistemi**
✅ **Cross-platform**

## 📚 Faydalı Linkler

- [VS Code C# Guide](https://code.visualstudio.com/docs/languages/csharp)
- [OmniSharp Documentation](https://github.com/OmniSharp/omnisharp-vscode)
- [.NET Framework in VS Code](https://code.visualstudio.com/docs/other/dotnet)
- [VS Code Debugging](https://code.visualstudio.com/docs/editor/debugging)

---
**Not**: .NET Framework projesi için VS Code, Visual Studio kadar entegre değildir ama test ve geliştirme için yeterlidir. 