using System.Diagnostics;

namespace Database
{
    public class DatabaseServiceTraditional : DatabaseService
    {
        public DatabaseServiceTraditional(ServiceConfiguration serviceConfiguration = null)
            : base(serviceConfiguration)
        { }

        public override Database CreateDatabase(ServiceConfiguration serviceConfiguration)
        {
            return DatabaseTraditional.Create(
                this,
                serviceConfiguration?.DataFilePath,
                serviceConfiguration?.LogFilePath);
        }

        public override void SnapWindow()
        {
            Window.SnapRight(Process.GetCurrentProcess());
        }
    }
}
