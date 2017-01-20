using Microsoft.Extensions.CommandLineUtils;
using System;

namespace DotNetToolbox
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication();

            app.HelpOption("-?|-h|--help");

            var installCommand = new InstallCommand(app);

            app.OnExecute(() =>
            {
                return 0;
            });

            return app.Execute(args);
        }
    }
}