using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace CloudVoiceCidService
{
    [RunInstaller(true)]
    public partial class ServiceInstaller : System.Configuration.Install.Installer
    {
        private ServiceProcessInstaller serviceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller serviceInstaller;

        public ServiceInstaller()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.serviceProcessInstaller = new ServiceProcessInstaller();
            this.serviceInstaller = new System.ServiceProcess.ServiceInstaller();

            // Service Process Installer
            this.serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            this.serviceProcessInstaller.Username = null;
            this.serviceProcessInstaller.Password = null;

            // Service Installer
            this.serviceInstaller.ServiceName = "CloudVoiceCidService";
            this.serviceInstaller.DisplayName = "CloudVoice CID Service";
            this.serviceInstaller.Description = "Cloud Voice sisteminden gelen aramaları COM4 portuna Caller ID formatında gönderen servis";
            this.serviceInstaller.StartType = ServiceStartMode.Automatic;

            // Installers
            this.Installers.AddRange(new Installer[] {
                this.serviceProcessInstaller,
                this.serviceInstaller
            });
        }

        protected override void OnAfterInstall(System.Collections.IDictionary savedState)
        {
            base.OnAfterInstall(savedState);
            
            // Servis kurulduktan sonra otomatik başlat
            try
            {
                using (ServiceController sc = new ServiceController("CloudVoiceCidService"))
                {
                    sc.Start();
                }
            }
            catch (Exception ex)
            {
                // Log yazma imkanı olmadığı için sessizce devam et
                // Servis manuel olarak başlatılabilir
            }
        }

        protected override void OnBeforeUninstall(System.Collections.IDictionary savedState)
        {
            // Servis kaldırılmadan önce durdur
            try
            {
                using (ServiceController sc = new ServiceController("CloudVoiceCidService"))
                {
                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    }
                }
            }
            catch (Exception ex)
            {
                // Durdurma hatası olursa devam et
            }
            
            base.OnBeforeUninstall(savedState);
        }
    }
} 