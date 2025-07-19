# ⚡ VS Code Hızlı Başlangıç

MSBuild hatası alıyorsunuz? İşte çözüm:

## 🚀 3 Adımda Test

### 1️⃣ Terminal Aç
```
Ctrl + ` (backtick)
```

### 2️⃣ Paketleri İndir
```cmd
DownloadPackages.bat
```

### 3️⃣ Derle ve Çalıştır
```cmd
BuildProject.bat
bin\Debug\CloudVoiceCidService.exe
```

## 🔧 Alternatif: Tasks Kullan

### Tasks Menüsü
```
Ctrl+Shift+P → "Tasks: Run Task"
```

### En İyi Seçenekler
- ✅ **"build-project"** ← En güvenilir
- ✅ **"download-packages"** ← Paket indirme
- ⚠️ "build" ← MSBuild path sorunu olabilir

## 🐛 Debug İçin

### F5 ile Debug
```
F5 → "Debug CloudVoice Service"
```

### Breakpoint Koyma
```
Kod satırının yanındaki numaraya tıkla
```

## 📋 Test Sonucu

### Console'da Göreceksiniz
```
==========================================
SERVİS BAŞLADI - TEST CALLER ID GÖNDERİLİYOR
==========================================
Test telefon numarası: +905551234567
...
```

### Eğer Hata Alırsanız
1. **COM4 port hatası**: Normal, test çevresinde
2. **Permission denied**: Admin olarak çalıştırın
3. **MSBuild not found**: BuildProject.bat kullanın

---
**Detaylı bilgi**: VSCode-Guide.md dosyasını açın 