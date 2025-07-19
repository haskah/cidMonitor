using System;
using System.IO.Ports;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic; // Added missing import
using System.Linq; // Added missing import

namespace CloudVoiceCidService
{
    public partial class CloudVoiceCidService : ServiceBase
    {
        private SerialPort serialPort;
        private HttpClient httpClient;
        private HttpListener webhookListener;
        private Timer pollTimer;
        private bool isRunning = false;
        
        // Debug/Test modu için
        public bool TestMode { get; set; } = false;
        
        // COM4 portunu hedefliyoruz - CidMonitor'ın dinlediği port
        private const string TARGET_COM_PORT = "COM4";
        private const int COM_BAUD_RATE = 1200;
        
        // Cloud Voice API ayarları (değiştirilebilir)
        private const string CLOUD_VOICE_API_URL = "https://your-cloud-voice-api.com/api/calls/latest";
        private const int WEBHOOK_PORT = 8080;
        private const int POLLING_INTERVAL = 3000; // 3 saniye
        
        public CloudVoiceCidService()
        {
            ServiceName = "CloudVoiceCidService";
            CanStop = true;
            CanPauseAndContinue = false;
            AutoLog = true;
        }
        
        protected override void OnStart(string[] args)
        {
            try
            {
                WriteLog("CloudVoice CID Service başlatılıyor...");
                
                // COM4 portunu aç
                InitializeComPort();
                
                // HTTP Client başlat
                httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                
                // Webhook listener başlat
                StartWebhookListener();
                
                // Polling timer başlat (alternatif/yedek)
                StartPollingTimer();
                
                isRunning = true;
                WriteLog($"CloudVoice CID Service başarıyla başlatıldı. COM Port: {TARGET_COM_PORT}");
                
                // Test numarası hemen gönder
                SendTestCallerID();
            }
            catch (Exception ex)
            {
                WriteLog($"Servis başlatma hatası: {ex.Message}");
                throw;
            }
        }
        
        protected override void OnStop()
        {
            try
            {
                isRunning = false;
                WriteLog("CloudVoice CID Service durduruluyor...");
                
                // Kaynakları temizle
                pollTimer?.Stop();
                pollTimer?.Dispose();
                
                webhookListener?.Stop();
                webhookListener?.Close();
                
                serialPort?.Close();
                serialPort?.Dispose();
                
                httpClient?.Dispose();
                
                WriteLog("CloudVoice CID Service başarıyla durduruldu.");
            }
            catch (Exception ex)
            {
                WriteLog($"Servis durdurma hatası: {ex.Message}");
            }
        }
        
        private void InitializeComPort()
        {
            try
            {
                serialPort = new SerialPort(TARGET_COM_PORT, COM_BAUD_RATE, Parity.None, 8, StopBits.One);
                serialPort.Handshake = Handshake.None;
                serialPort.DtrEnable = true;
                serialPort.RtsEnable = true;
                serialPort.ReadTimeout = 1000;
                serialPort.WriteTimeout = 1000;
                
                serialPort.Open();
                WriteLog($"COM Port {TARGET_COM_PORT} başarıyla açıldı.");
            }
            catch (Exception ex)
            {
                WriteLog($"COM Port açma hatası: {ex.Message}");
                throw;
            }
        }
        
        private void StartWebhookListener()
        {
            try
            {
                webhookListener = new HttpListener();
                webhookListener.Prefixes.Add($"http://localhost:{WEBHOOK_PORT}/cloudvoice/");
                webhookListener.Start();
                
                Task.Run(() => ProcessWebhookRequests());
                WriteLog($"Webhook listener başlatıldı: http://localhost:{WEBHOOK_PORT}/cloudvoice/");
            }
            catch (Exception ex)
            {
                WriteLog($"Webhook listener hatası: {ex.Message}");
            }
        }
        
        private async void ProcessWebhookRequests()
        {
            while (isRunning && webhookListener.IsListening)
            {
                try
                {
                    var context = await webhookListener.GetContextAsync();
                    var request = context.Request;
                    
                    WriteLog("Webhook isteği alındı...");
                    
                    // POST verilerini oku
                    string requestBody = "";
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        requestBody = await reader.ReadToEndAsync();
                    }
                    
                    WriteLog($"Webhook payload: {requestBody}");
                    
                    // Telefon numarasını çıkar
                    string phoneNumber = ExtractPhoneNumberFromPayload(requestBody);
                    
                    if (!string.IsNullOrEmpty(phoneNumber))
                    {
                        WriteLog($"Telefon numarası bulundu: {phoneNumber}");
                        await SendCallerIdToComPort(phoneNumber);
                    }
                    
                    // Response gönder
                    var response = context.Response;
                    response.StatusCode = 200;
                    byte[] responseBytes = Encoding.UTF8.GetBytes("{\"status\":\"success\"}");
                    response.ContentLength64 = responseBytes.Length;
                    await response.OutputStream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    response.Close();
                }
                catch (Exception ex)
                {
                    WriteLog($"Webhook işleme hatası: {ex.Message}");
                }
            }
        }
        
        private void StartPollingTimer()
        {
            pollTimer = new Timer(async (state) =>
            {
                if (isRunning)
                {
                    await CheckCloudVoiceForNewCalls();
                }
            }, null, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(POLLING_INTERVAL));
        }
        
        private async Task CheckCloudVoiceForNewCalls()
        {
            try
            {
                // Cloud Voice API'den yeni aramaları kontrol et
                var response = await httpClient.GetStringAsync(CLOUD_VOICE_API_URL);
                
                // JSON response'u parse et
                dynamic callData = JsonConvert.DeserializeObject(response);
                
                if (callData?.phoneNumber != null)
                {
                    string phoneNumber = callData.phoneNumber.ToString();
                    WriteLog($"API'den telefon numarası alındı: {phoneNumber}");
                    await SendCallerIdToComPort(phoneNumber);
                }
            }
            catch (HttpRequestException)
            {
                // API ulaşılamıyor, normal durum - log yazma
            }
            catch (Exception ex)
            {
                WriteLog($"API kontrolü hatası: {ex.Message}");
            }
        }
        
        private string ExtractPhoneNumberFromPayload(string payload)
        {
            try
            {
                // JSON formatında geliyorsa
                if (payload.Trim().StartsWith("{"))
                {
                    dynamic data = JsonConvert.DeserializeObject(payload);
                    return data?.phoneNumber?.ToString() ?? 
                           data?.caller?.ToString() ?? 
                           data?.from?.ToString() ?? 
                           data?.phone?.ToString();
                }
                
                // URL encoded formatında geliyorsa
                if (payload.Contains("phone") || payload.Contains("caller"))
                {
                    var parts = payload.Split('&');
                    foreach (var part in parts)
                    {
                        var keyValue = part.Split('=');
                        if (keyValue.Length == 2)
                        {
                            var key = keyValue[0].ToLower();
                            if (key.Contains("phone") || key.Contains("caller") || key == "from")
                            {
                                return WebUtility.UrlDecode(keyValue[1]);
                            }
                        }
                    }
                }
                
                // Direkt numara gönderilmişse
                if (IsPhoneNumber(payload.Trim()))
                {
                    return payload.Trim();
                }
            }
            catch (Exception ex)
            {
                WriteLog($"Telefon numarası çıkarma hatası: {ex.Message}");
            }
            
            return null;
        }
        
        private bool IsPhoneNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            
            // + ile başlayabilir, sadece rakam, +, -, (, ), space içerebilir
            string cleaned = input.Replace("+", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "");
            
            return cleaned.Length >= 7 && cleaned.Length <= 15 && cleaned.All(char.IsDigit);
        }
        
        private async Task SendCallerIdToComPort(string phoneNumber)
        {
            try
            {
                if (serialPort == null || !serialPort.IsOpen)
                {
                    WriteLog("COM Port kapalı, yeniden açılıyor...");
                    InitializeComPort();
                }
                
                WriteLog($"Caller ID gönderiliyor: {phoneNumber}");
                
                // Bell 202 FSK Caller ID mesajı oluştur
                byte[] callerIdMessage = BuildCallerIdMessage(phoneNumber);
                
                // COM porta gönder
                await Task.Run(() =>
                {
                    serialPort.Write(callerIdMessage, 0, callerIdMessage.Length);
                });
                
                WriteLog($"Caller ID başarıyla gönderildi: {phoneNumber}");
            }
            catch (Exception ex)
            {
                WriteLog($"COM Port gönderme hatası: {ex.Message}");
            }
        }
        
        private byte[] BuildCallerIdMessage(string phoneNumber)
        {
            var message = new List<byte>();
            DateTime now = DateTime.Now;
            
            // Channel Seizure - 30 byte 0x55 (01010101 pattern)
            for (int i = 0; i < 30; i++)
            {
                message.Add(0x55);
            }
            
            // Mark Signal - 18 byte 0xFF
            for (int i = 0; i < 18; i++)
            {
                message.Add(0xFF);
            }
            
            // Message Frame başlangıcı
            message.Add(0x16); // DLE (Data Link Escape)
            message.Add(0x01); // SOH (Start of Header)
            message.Add(0x80); // Message Type (Single Data Message)
            
            // Data section oluştur
            var dataSection = new List<byte>();
            
            // Parameter 1: Date and Time
            dataSection.Add(0x01); // Parameter type: Date/Time
            dataSection.Add(0x08); // Length: 8 bytes
            string dateTime = now.ToString("MMddHHmm"); // MMDDHHMM format
            dataSection.AddRange(Encoding.ASCII.GetBytes(dateTime));
            
            // Parameter 2: Phone Number
            dataSection.Add(0x02); // Parameter type: Calling Number
            dataSection.Add((byte)phoneNumber.Length); // Length
            dataSection.AddRange(Encoding.ASCII.GetBytes(phoneNumber));
            
            // Message length ve data section'ı ekle
            message.Add((byte)dataSection.Count);
            message.AddRange(dataSection);
            
            // Message frame sonu
            message.Add(0x16); // DLE
            message.Add(0x03); // ETX (End of Text)
            
            // Checksum hesapla (SOH'dan ETX'e kadar)
            int checksum = 0;
            for (int i = 31; i < message.Count; i++) // Channel seizure ve mark'ı atla
            {
                checksum += message[i];
            }
            checksum = (-checksum) & 0xFF;
            message.Add((byte)checksum);
            
            return message.ToArray();
        }
        
        private void SendTestCallerID()
        {
            try
            {
                // App.config'den test numarasını oku
                string testPhoneNumber = System.Configuration.ConfigurationManager.AppSettings["TestPhoneNumber"] ?? "+905551234567";
                bool enableTestMode = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableTestMode"] ?? "true");
                
                if (enableTestMode)
                {
                    WriteLog("==========================================");
                    WriteLog("SERVİS BAŞLADI - TEST CALLER ID GÖNDERİLİYOR");
                    WriteLog("==========================================");
                    WriteLog($"Test telefon numarası: {testPhoneNumber}");
                    WriteLog($"Hedef COM Port: {TARGET_COM_PORT}");
                    WriteLog("Bu test Samba CidMonitor'ın düzgün çalıştığını doğrular.");
                    
                    // Test numarasını hemen gönder
                    SendCallerIdToComPort(testPhoneNumber).Wait();
                    
                    WriteLog("==========================================");
                    WriteLog("TEST CALLER ID GÖNDERİLDİ");
                    WriteLog("==========================================");
                    WriteLog("Eğer Samba'da popup çıktıysa sistem çalışıyor!");
                }
                else
                {
                    WriteLog("Test modu kapalı - test numarası gönderilmedi.");
                }
            }
            catch (Exception ex)
            {
                WriteLog($"Test Caller ID hatası: {ex.Message}");
                WriteLog("Test hatası normal, servis çalışmaya devam edecek.");
            }
        }
        
        private void WriteLog(string message)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logMessage = $"[{timestamp}] {message}";
                
                // Event Log'a yaz
                if (!EventLog.SourceExists("CloudVoiceCidService"))
                {
                    EventLog.CreateEventSource("CloudVoiceCidService", "Application");
                }
                EventLog.WriteEntry("CloudVoiceCidService", logMessage, EventLogEntryType.Information);
                
                // Dosyaya da yaz
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CloudVoiceCid.log");
                File.AppendAllText(logPath, logMessage + Environment.NewLine);
            }
            catch
            {
                // Log yazma hatası olursa sessizce devam et
            }
        }
    }
} 