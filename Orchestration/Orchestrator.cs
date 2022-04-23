using System;
using System.Reflection;
using System.Threading;

namespace Database
{
    public abstract class Orchestrator
    {
        private CancellationTokenSource KeepServicesUpThreadCancellationTokenSource;
        public DatabaseService DatabaseService { get; protected set; }

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
            DatabaseService.Process.Kill();
        }

        protected void KeepServiceUp<TServiceAction, TServiceRequest, TServiceResponseResult>(
            Service<TServiceAction, TServiceRequest, TServiceResponseResult> service)
            where TServiceAction : Enum
            where TServiceRequest : ServiceRequest<TServiceAction>
            where TServiceResponseResult : ServiceResponseResult
        {
            Thread.CurrentThread.Name = service.GetType() + "_" + MethodBase.GetCurrentMethod().Name;

            while (true)
            {
                Utility.TraceDebugMessage(string.Format("Starting up {0}...", service.GetType()));
                service.StartAsProcess();
                service.Process.WaitForExit();
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

        protected abstract void WaitForServicesBoot();

        protected abstract void StopStartedServices();

        protected abstract void SnapWindow();
    }
}
