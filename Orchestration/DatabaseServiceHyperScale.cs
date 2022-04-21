using System.Diagnostics;

namespace Database
{
    public class DatabaseServiceHyperscale : DatabaseService
    {
        public DatabaseServiceHyperscale(ServiceConfiguration serviceConfiguration = null)
            : base(serviceConfiguration)
        { }

        public override void SnapWindow()
        {
            Window.SnapTopRight(Process.GetCurrentProcess());
        }
    }
}
