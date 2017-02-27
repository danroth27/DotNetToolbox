﻿using Microsoft.Extensions.CommandLineUtils;
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
            var listCommand = new ListCommand(app);
            var uninstallCommand = new UninstallCommand(app);

            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 0;
            });

            return app.Execute(args);
        }
    }
}