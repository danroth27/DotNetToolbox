using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetToolbox
{
    public class VersionOption : CommandOption
    {
        public VersionOption(CommandLineApplication app) : base("-v|--version", CommandOptionType.SingleValue)
        {
            App = app;
            Description = "Tool package version";
        }

        public CommandLineApplication App { get; set; }
    }
}
