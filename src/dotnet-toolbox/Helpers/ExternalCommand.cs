using System;
using System.Diagnostics;

namespace DotNetToolbox.Helpers
{
    public class ExternalCommand
    {
        private Process _process;

        private ExternalCommand(string name, string args)
        {
            var data = new ProcessStartInfo
            {
                FileName = name,
                Arguments = args
            };
            _process = new Process
            {
                StartInfo = data
            };
        }

        public ExternalCommand Execute()
        {
            _process.Start();
            _process.WaitForExit();
            return this;
        }

        public void EnsureSuccessful(string message = "")
        {
            if (_process.ExitCode != 0)
            {
                throw new Exception($"Something weird happened and the error is {message}");
            }
        }

        public static ExternalCommand Create(string name, params string[] arguments)
        {
            return new ExternalCommand(name, string.Join(" ", arguments));
        }

        public ExternalCommand CaptureStandardOut()
        {
            _process.StartInfo.RedirectStandardOutput = true;
            return this;
        }
    }
}
