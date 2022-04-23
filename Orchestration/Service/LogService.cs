using System;
using System.Diagnostics;

namespace Database
{
    public class LogService : Service
    {
        public LogService(ServiceConfiguration serviceConfiguration = null)
            : base(serviceConfiguration)
        { }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public override void SnapWindow()
        {
            Window.SnapBottomRight(Process.GetCurrentProcess());
        }
    }
}
