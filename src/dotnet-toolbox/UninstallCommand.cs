using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DotNetToolbox.Helpers;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using DotNetToolbox.VersionMetadata;

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
            var packageId = PackageArgument.Value;

            Out.WriteLine($"Attempting to uninstall {packageId}");
            try
            {
                //var binaryName = String.Format("{0}{1}", GetBinaryNameForPackage(packageId), GetExtension());
                var existingPackage = VersionFile.Get((PackageMetadata p) => p.PackageId == packageId || p.BinaryName == packageId, _toolboxConfig.VersionsFile);
                if (existingPackage == null)
                {
                    this.Die($"Specified item ({packageId}) does not exist.");
                }
                RemoveScriptBinding(existingPackage.BinaryName);
                //RemoveFromVersionsFile(packageId);
                VersionFile.RemoveVersion(existingPackage, _toolboxConfig.VersionsFile);
                Out.WriteLine($"{packageId} uninstalled successfully!");
                return 0;
            }
            catch (Exception ex)
            {
                this.Die(ex.Message, 100);
                return 1;
            }
        }

        //private string GetBinaryNameForPackage(string packageId)
        //{
        //    var installedVersion = GetInstalledVersionForPackageId(packageId);
        //    var fullPath = Path.Combine(_toolboxConfig.NugetPackageRoot, packageId, installedVersion);
        //    var fileName = Directory.GetFiles(fullPath, "dotnet-*.dll", SearchOption.AllDirectories).FirstOrDefault();
        //    if (String.IsNullOrEmpty(fileName))
        //        this.Die($"There is no dontet-*.dll binary associated with {packageId}.");
        //    return Path.GetFileNameWithoutExtension(fileName);
        //}

        private string GetExtension()
        {
            return (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? ".cmd" : "";
        }

        void RemoveScriptBinding(string binaryName)
        {
            File.Delete(Path.Combine(_toolboxConfig.ToolboxDirectoryPath, $"{binaryName}{GetExtension()}"));
        }

        //void RemoveFromVersionsFile(string packageId)
        //{
        //    var remainingTools = JsonConvert.DeserializeObject<List<PackageMetadata>>(File.ReadAllText(_toolboxConfig.VersionsFile))
        //                                    .Where(p => !String.Equals(p.PackageId, packageId, StringComparison.OrdinalIgnoreCase)).ToList();
        //    File.WriteAllText(_toolboxConfig.VersionsFile, JsonConvert.SerializeObject(remainingTools));
        //}

        //private string GetInstalledVersionForPackageId(string packageId)
        //{
        //    var installedPackage = JsonConvert.DeserializeObject<List<PackageMetadata>>(File.ReadAllText(_toolboxConfig.VersionsFile))
        //                                      .FirstOrDefault(p => p.PackageId == packageId);
        //    if (installedPackage == null)
        //        this.Die($"There is no entry for {packageId} in the metadata file.");
        //    return installedPackage.RestoredVersion;
        //}
    }
}
