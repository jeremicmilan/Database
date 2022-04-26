using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Database
{
    public abstract class Orchestrator
    {
        private CancellationTokenSource KeepServicesUpThreadCancellationTokenSource;
        public DatabaseService DatabaseService { get; protected set; }

        protected List<Service> Services = new List<Service>();

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

        public void DatabaseRestart()
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
            DatabaseService.OverrideConfiguration(serviceConfiguration);

            // Restart database so new configuration is picked up.
            //
            DatabaseRestart();
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
