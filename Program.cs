using System;
using System.ServiceProcess;

namespace CloudVoiceCidService
{
    static class Program
    {
        /// <summary>
        /// CloudVoice CID Service - COM4 portuna Caller ID verisi gönderen Windows servisi
        /// </summary>
        static void Main(string[] args)
        {
            // Debug modunda çalıştırılıyorsa (Visual Studio'dan F5 ile)
            if (Environment.UserInteractive)
            {
                Console.WriteLine("CloudVoice CID Service - Debug Mode");
                Console.WriteLine("COM4 portuna Caller ID verisi gönderen servis");
                Console.WriteLine("Servis olarak çalıştırmak için:");
                Console.WriteLine("  InstallService.bat - Servisi kur");
                Console.WriteLine("  UninstallService.bat - Servisi kaldır");
                Console.WriteLine();
                Console.WriteLine("Test için Enter'a basın...");
                Console.ReadLine();
                
                // Debug modunda servisi test et
                var service = new CloudVoiceCidService();
                service.TestMode = true;
                
                try
                {
                    // Servisi başlat (OnStart'ı çağır)
                    var onStart = typeof(CloudVoiceCidService).GetMethod("OnStart", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    onStart?.Invoke(service, new object[] { new string[0] });
                    
                    Console.WriteLine("Servis başlatıldı. Çıkmak için Enter'a basın...");
                    Console.ReadLine();
                    
                    // Servisi durdur (OnStop'u çağır)
                    var onStop = typeof(CloudVoiceCidService).GetMethod("OnStop", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    onStop?.Invoke(service, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Hata: {ex.Message}");
                    Console.WriteLine("Devam etmek için Enter'a basın...");
                    Console.ReadLine();
                }
            }
            else
            {
                // Windows Service olarak çalıştır
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new CloudVoiceCidService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
} 