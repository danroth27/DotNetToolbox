using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotNetToolbox
{
    public class InstallCommand : CommandLineApplication
    {
        public InstallCommand(CommandLineApplication parent)
        {
            Parent = parent;
            Name = "install";
            PackageArgument = new PackageArgument(this);
            Arguments.Add(PackageArgument);
            VersionOption = new VersionOption(this);
            Options.Add(VersionOption);
            OnExecute((Func<Task<int>>)Run);
            Parent.Commands.Add(this);
        }

        public PackageArgument PackageArgument { get; set; }
        public VersionOption VersionOption { get; set; }

        public async Task<int> Run()
        {
            var packageId = PackageArgument.Value;

            Out.WriteLine($"I wish I could install {packageId}, but I don't know how! :'(");

            return 0;
        }
    }
}
