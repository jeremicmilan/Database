﻿using System.Diagnostics;

namespace Database
{
    public class DatabaseServiceHyperscale : DatabaseService
    {
        public DatabaseServiceHyperscale(ServiceConfiguration serviceConfiguration = null)
            : base(serviceConfiguration)
        { }

        public override Database CreateDatabase(ServiceConfiguration serviceConfiguration)
        {
            return DatabaseHyperscale.Create(
                this,
                serviceConfiguration?.LogFilePath);
        }

        public override void SnapWindow()
        {
            Window.SnapTopRight(Process.GetCurrentProcess());
        }
    }
}
