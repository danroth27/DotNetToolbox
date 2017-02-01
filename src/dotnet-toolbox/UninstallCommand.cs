using System;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;

namespace DotNetToolbox
{
    public class UninstallCommand : CommandLineApplication
    {
        private ToolboxConfiguration _toolboxConfig;
        public UninstallCommand(CommandLineApplication parent)
        {
            Parent = parent;
            Name = "install";
            PackageArgument = new PackageArgument(this);
            Arguments.Add(PackageArgument);
            OnExecute((Func<Task<int>>)Run);
            Parent.Commands.Add(this);
            HelpOption("-h|--help");
            _toolboxConfig = new ToolboxConfiguration();

        }
        public PackageArgument PackageArgument { get; set; }

        public async Task<int> Run()
        {
            var pkg = new PackageMetadata(PackageArgument.Value);

            return 0;
        }

    }
}
