using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public abstract class Service
    {

        public Process Process = null;

        public abstract void StartUp();

        public void StartUpAsProcess()
        {
            Process currentProcess = Process.GetCurrentProcess();

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.UseShellExecute = true;

            string processName = currentProcess.ProcessName;
            string arguments = this.GetType().ToString();
            processStartInfo.FileName = processName;
            processStartInfo.Arguments = arguments;

            Process = Process.Start(processStartInfo);

            Console.WriteLine(string.Format("Process {0} started with arguments {1}", processName, arguments));
        }
    }
}
