using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetToolbox
{
    public class PackageArgument : CommandArgument
    {
        public PackageArgument(CommandLineApplication app)
        {
            App = app;
            Name = "Package";
            Description = "Tool package identifier";
        }

        public CommandLineApplication App { get; set; }
    }
}
