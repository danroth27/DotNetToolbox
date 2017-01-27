using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetToolbox
{
    public class PackageVersionOption : CommandOption
    {
        public PackageVersionOption(CommandLineApplication app) : base("-v|--version", CommandOptionType.SingleValue)
        {
            App = app;
            Description = "Tool package version";
            App.Options.Add(this);
        }

        public CommandLineApplication App { get; set; }
    }
}
