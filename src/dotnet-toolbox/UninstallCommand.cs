using System;
using System.Runtime.InteropServices;
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

            // Here we need to...
            // 1. Get the binary name based on the package metadata
            // 2. Get the scrpt name based on the OS
            // 3. Remove the script named thus
            // 4. Remove the entry from the version file
            Out.WriteLine($"Attempting to uninstall {pkg.PackageId}");
            var binaryName = GetBinaryNameForPackage(pkg.PackageId);
            RemoveScriptBinding(binaryName);
            Out.WriteLine($"{pkg.PackageId} uninstalled successfully!");
            return 0;
        }

        private string GetBinaryNameForPackage(string packageId)
        {
            throw new NotImplementedException();
        }

        private string GetExtension()
        {
            return (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? ".cmd" : ".sh";
        }

        void RemoveScriptBinding(string binaryName)
        {
            throw new NotImplementedException();
        }
    }
}
