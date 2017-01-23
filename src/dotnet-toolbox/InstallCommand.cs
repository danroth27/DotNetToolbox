using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using NuGet.Configuration;
using System.Linq;
using Microsoft.Build.Construction;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol;
using System.Threading;
using NuGet.Common;

namespace DotNetToolbox
{
    public class InstallCommand : CommandLineApplication
    {
        private const string _directory = ".toolbox";
        private const string _projectFileName = "globaltools.csproj";
        private ToolboxPaths _toolboxPaths;
        private string _projectFile = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netcoreapp1.0</TargetFramework>
  </PropertyGroup>
</Project>";
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
            HelpOption("-h|--help");
            _toolboxPaths = new ToolboxPaths();
        }

        public PackageArgument PackageArgument { get; set; }
        public VersionOption VersionOption { get; set; }

        public async Task<int> Run()
        {
            var packageId = PackageArgument.Value;
            Out.WriteLine($"The tool ID to install: {packageId}");
            Out.WriteLine("Determining version...");
            var packageVersion = VersionOption.HasValue() ? VersionOption.Value() : await ResolveLatestFromNuget(packageId);

            // Get the paths
            //var paths = GetDirAndProjectPaths();
            EnsureProjectExists();
            
            // Add the ItemGroup
            var projectRoot = ProjectRootElement.Open(_toolboxPaths.ToolboxProjectPath);
            var itemGroup = projectRoot.AddItemGroup();
            itemGroup.AddItem("DotNetCliToolReference", packageId, new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Version", packageVersion)});
            projectRoot.Save();
            Out.WriteLine($"Added {packageId} to the store...");
            // Call restore 
            Out.WriteLine("Installing the package...");
            var restore = Process.Start(new ProcessStartInfo {
                FileName = "dotnet",
                Arguments = $"restore {_toolboxPaths.ToolboxProjectPath}",
                RedirectStandardOutput = true,
                RedirectStandardError = false
            });
            restore.WaitForExit();

            // We have the package restored
            var nugetPackagePath = NuGetPathContext.Create(settingsRoot: String.Empty).UserPackageFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var fullPath = Path.Combine(nugetPackagePath, packageId, packageVersion, "lib", "netcoreapp1.0");
            //Out.WriteLine(fullPath);

            // Find the dotnet-<foo> File
            var pathToTool = Directory.GetFiles(fullPath, "dotnet*.dll").FirstOrDefault();
            if (string.IsNullOrEmpty(pathToTool))
                throw new Exception("The package does not contain a dotnet-*.dll file");

            Out.WriteLine("Putting the tool into the path...");
            PutToolIntoPath(nugetPackagePath, pathToTool);
            Out.WriteLine("Task Completed!");
            return 0;

        }

        private async Task<string> ResolveLatestFromNuget(string packageId)
        {
            var repo = Repository.Factory.GetCoreV3(NuGetConstants.V3FeedUrl);
            var resource = repo.GetResource<MetadataResource>();
            var version = await resource.GetLatestVersion(packageId, true, false, NullLogger.Instance, CancellationToken.None);
            return version.ToString();
        }

        private void PutToolIntoPath(string nugetPackagePath, string pathToTool)
        {
            var script = new StringBuilder();
            var scriptPath = Path.Combine(_toolboxPaths.ToolboxDirectoryPath, Path.GetFileNameWithoutExtension(pathToTool));
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                script.AppendLine($"dotnet --additionalprobingpath {nugetPackagePath} {pathToTool}");
                scriptPath += ".cmd";
            }
            else
            {
                script.AppendLine("#!/bin/sh");
                script.AppendLine($"dotnet --additionalprobingpath {nugetPackagePath} {pathToTool}");
            }

            try
            {
                File.WriteAllText($"{scriptPath}", script.ToString());
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var chmod = Process.Start(new ProcessStartInfo
                    {
                        FileName = "chmod",
                        Arguments = $"+x {scriptPath}",
                        RedirectStandardOutput = true
                    });
                    chmod.WaitForExit();
                    if (chmod.ExitCode != 0)
                    {
                        Error.WriteLine($"There was an error installing the tool: {chmod.StandardError.ReadToEnd()}");
                    }
                }
            }
            catch (Exception e)
            {
                Error.WriteLine($"There was an error installing the tool. The error returned was: {e.Message}");
            }

        }
        private void EnsureProjectExists()
        {
            //var paths = GetDirAndProjectPaths();
            try
            {
                if (!Directory.Exists(_toolboxPaths.ToolboxDirectoryPath))
                {
                    Directory.CreateDirectory(_toolboxPaths.ToolboxDirectoryPath);
                }
                if (!File.Exists(_toolboxPaths.ToolboxProjectPath))
                {
                    File.WriteAllText(_toolboxPaths.ToolboxProjectPath, _projectFile);
                }
            }
            catch (Exception e)
            {
                Error.WriteLine(e.Message);
            }

        }
    }
}
