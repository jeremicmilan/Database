using System;
using System.Diagnostics;

namespace Database
{
    public class StorageService : Service
    {
        public StorageService(ServiceConfiguration serviceConfiguration = null)
            : base(serviceConfiguration)
        { }

        public override void StartUp()
        {
            throw new NotImplementedException();
        }

        public override void SnapWindow()
        {
            Window.SnapBottomLeft(Process.GetCurrentProcess());
        }
    }
}
