using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DotNetToolbox.Helpers;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace DotNetToolbox
{
    public class UninstallCommand : CommandLineApplication
    {
        private ToolboxConfiguration _toolboxConfig;
        public UninstallCommand(CommandLineApplication parent)
        {
            Parent = parent;
            Name = "uninstall";
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
            try
            {
                var binaryName = String.Format("{0}{1}", GetBinaryNameForPackage(pkg.PackageId), GetExtension());
                RemoveScriptBinding(binaryName);
                RemoveFromVersionsFile(pkg.PackageId);
                Out.WriteLine($"{pkg.PackageId} uninstalled successfully!");
                return 0;
            }
            catch (Exception ex)
            {
                this.Die(ex.Message, 100);
                return 1;
            }
        }

        private string GetBinaryNameForPackage(string packageId)
        {
            // Go to the NuGet folder
            // Find the package
            // Find the version folder (need to get the info from the metadata file)
            // Find the binary since it has to be called dotnet-<foo> and there has to be one
            // return that mofo
            // Delete from the versions file since it is dead, right?
            //throw new NotImplementedException();
            var installedVersion = GetInstalledVersionForPackageId(packageId);
            var fullPath = Path.Combine(_toolboxConfig.NugetPackageRoot, packageId, installedVersion);
            var fileName = Directory.GetFiles(fullPath, "dotnet-*.dll", SearchOption.AllDirectories).FirstOrDefault();
            if (String.IsNullOrEmpty(fileName))
                this.Die($"There is no dontet-*.dll binary associated with {packageId}.");
            return Path.GetFileNameWithoutExtension(fileName);
        }

        private string GetExtension()
        {
            return (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? ".cmd" : "";
        }

        void RemoveScriptBinding(string binaryName)
        {
            File.Delete(Path.Combine(_toolboxConfig.ToolboxDirectoryPath, binaryName));
        }

        void RemoveFromVersionsFile(string packageId)
        {
            var remainingTools = JsonConvert.DeserializeObject<List<PackageMetadata>>(File.ReadAllText(_toolboxConfig.VersionsFile))
                                            .Where(p => !String.Equals(p.PackageId, packageId, StringComparison.OrdinalIgnoreCase)).ToList();
            File.WriteAllText(_toolboxConfig.VersionsFile, JsonConvert.SerializeObject(remainingTools));
        }

        private string GetInstalledVersionForPackageId(string packageId)
        {
            var installedPackage = JsonConvert.DeserializeObject<List<PackageMetadata>>(File.ReadAllText(_toolboxConfig.VersionsFile))
                                              .FirstOrDefault(p => p.PackageId == packageId);
            if (installedPackage == null)
                this.Die($"There is no entry for {packageId} in the metadata file.");
            return installedPackage.RestoredVersion;
        }
    }
}
