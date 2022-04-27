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
            Thread.CurrentThread.Name = service.GetType() + "_" + MethodBase.GetCurrentMethod().Name;

            while (true)
            {
                Utility.TraceDebugMessage(string.Format("Starting up {0}...", service.GetType()));
                service.StartAsProcess();
                service.WaitForExit();
                Utility.TraceDebugMessage(string.Format("{0} exited.", service.GetType()));

                if (KeepServicesUpThreadCancellationTokenSource != null &&
                    KeepServicesUpThreadCancellationTokenSource.IsCancellationRequested)
                {
                    break;
                }

                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
        }

        public void OverrideDatabaseServiceConfiguration(ServiceConfiguration serviceConfiguration)
        {
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
                service.IsServiceUp();
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
