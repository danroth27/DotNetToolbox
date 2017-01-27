using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetToolbox
{
    public class SourceOption : CommandOption
    {
        public SourceOption(CommandLineApplication app) : base("-s|--source", CommandOptionType.SingleValue)
        {
            App = app;
            Description = "Specifies a NuGet package source to use to install tools";
            App.Options.Add(this);
        }

        public CommandLineApplication App { get; set; }
    }
}
