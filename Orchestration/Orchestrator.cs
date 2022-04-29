using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Database
{
    public abstract class Orchestrator
    {
        private CancellationTokenSource KeepServicesUpThreadCancellationTokenSource;
        public DatabaseService DatabaseService => GetService<DatabaseService>();

        protected List<Service> Services = new List<Service>();
        protected TService GetService<TService>()
            where TService : Service
        {
            return (TService)Services.Where(service => service.GetType().IsSubclassOf(typeof(TService))).FirstOrDefault();
        }

        public void Start()
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Stop();

            SnapWindow();

            KeepServicesUpThreadCancellationTokenSource = new CancellationTokenSource();
            StartServices();
            WaitForServicesBoot();
        }

        public void Stop()
        {
            KeepServicesUpThreadCancellationTokenSource?.Cancel();
            StopStartedServices();
        }

        public abstract void KillAllServices();

        public void KillDatabaseService()
        {
            DatabaseService.Kill();
        }

        protected void KeepServiceUp(Service service)
        {
            Thread.CurrentThread.Name = service.GetType().ToString()[9..] + "_" + MethodBase.GetCurrentMethod().Name;

            while (true)
            {
                Utility.LogServiceBegin(string.Format("Starting up {0}...", service.GetType().ToString()[9..]));
                service.StartAsProcess();
                Utility.LogServiceEnd(string.Format("{0} started.", service.GetType().ToString()[9..]));
                service.WaitForExit();
                Utility.LogServiceEnd(string.Format("{0} exited.", service.GetType().ToString()[9..]));

                if (KeepServicesUpThreadCancellationTokenSource != null &&
                    KeepServicesUpThreadCancellationTokenSource.IsCancellationRequested)
                {
                    break;
                }

                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
        }

        public void OverrideServicesConfiguration(ServiceConfiguration serviceConfiguration)
        {
            Utility.LogMessage("Overriding services configuration with configuration.");

            foreach (Service service in Services)
            {
                service.OverrideConfiguration(serviceConfiguration);

                // Restart service so new configuration is picked up.
                //
                service.Kill();
            }
        }

        protected abstract void StartServices();

        protected void WaitForServicesBoot()
        {
            foreach (Service service in Services)
            {
                Utility.WaitUntil(() => service.IsWaitingForExit);
            }
        }

        protected void StopStartedServices()
        {
            foreach (Service service in Services)
            {
                service.Stop();
            }

            Services.Clear();
        }

        protected abstract void SnapWindow();
    }
}
