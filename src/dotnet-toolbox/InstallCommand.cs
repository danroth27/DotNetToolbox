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

namespace DotNetToolbox
{
    public class InstallCommand : CommandLineApplication
    {
        private const string _directory = ".toolbox";
        private const string _projectFileName = "globaltools.csproj";
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
        }

        public PackageArgument PackageArgument { get; set; }
        public VersionOption VersionOption { get; set; }

        public async Task<int> Run()
        {
            var packageId = PackageArgument.Value;
            if (!VersionOption.HasValue())
            {
                Error.WriteLine("The package version needs to be specified in the invocation");
                return 1;
            }
            var packageVersion = VersionOption.Value();
            Out.WriteLine($"The package ID provided was {packageId}");

            // Get the paths
            var paths = GetDirAndProjectPaths();
            EnsureProjectExists(paths);
            
            // Add the ItemGroup
            var projectRoot = ProjectRootElement.Open(paths.Item2);
            var itemGroup = projectRoot.AddItemGroup();
            itemGroup.AddItem("DotNetCliToolReference", packageId, new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Version", packageVersion)});
            projectRoot.Save();
            Out.WriteLine($"Added {packageId} to the store...");
            // Call restore 
            Out.WriteLine("Installing the package...");
            var restore = Process.Start(new ProcessStartInfo {
                FileName = "dotnet",
                Arguments = $"restore {paths.Item2}",
                RedirectStandardOutput = false,
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

            Out.WriteLine($"dotnet --additionalprobingpath {nugetPackagePath} {pathToTool}");

            // Now we have the thing and we need to execute it using dotnet to make sure it works
            //var p2 = Process.Start(new ProcessStartInfo
            //{
            //    FileName = "dotnet",
            //    Arguments = $"--additionalprobingpath {nugetPackagePath} {pathToTool}",
            //});
            //p2.WaitForExit();

            // Out.WriteLine($"I wish I could install {packageId}, but I don't know how! :'(");
            // Out.WriteLine($"The home dir is {homeDir}");
            Out.WriteLine("Putting the tool into the path...");
            PutToolIntoPath(nugetPackagePath, pathToTool);
            //var repo = Repository.Factory.GetCoreV3(NuGetConstants.V3FeedUrl);
            //var resource = await repo.GetResourceAsync<MetadataResource>();
            //resource.GetLatestVersion(string packageId, bool includePrerelease, bool includeUnlisted, ILogger log, CancellationToken token);
            Out.WriteLine("Task Completed!");
            return 0;

        }

        private void PutToolIntoPath(string nugetPackagePath, string pathToTool)
        {
            var script = new StringBuilder();
            var scriptName = Path.GetFileNameWithoutExtension(pathToTool);
            var homeDir = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? Environment.GetEnvironmentVariable("USERPROFILE") : Environment.GetEnvironmentVariable("HOME");
            var dotnet = Path.Combine(homeDir, _directory);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Assume that the .toolbox is in the $PATH
                script.AppendLine($"dotnet --additionalprobingpath {nugetPackagePath} {pathToTool}");
                File.WriteAllText($"{Path.Combine(dotnet, scriptName)}.cmd", script.ToString());
            }
            else
            {
                script.AppendLine("#!/bin/sh");
                script.AppendLine($"dotnet --additionalprobingpath {nugetPackagePath} {pathToTool}");
                // Write it out
            }
            
        }

        private Tuple<string, string> GetDirAndProjectPaths()
        {
            var homeDir = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? Environment.GetEnvironmentVariable("USERPROFILE") : Environment.GetEnvironmentVariable("HOME");
            var dotnet = Path.Combine(homeDir, _directory);
            var projectFile = Path.Combine(dotnet, _projectFileName);
            return new Tuple<string, string>(dotnet, projectFile);
        }

        private void EnsureProjectExists(Tuple<string, string> paths)
        {
            //var paths = GetDirAndProjectPaths();
            try
            {
                if (!Directory.Exists(paths.Item1))
                {
                    Directory.CreateDirectory(paths.Item1);
                }
                if (!File.Exists(paths.Item2))
                {
                    File.WriteAllText(paths.Item2, _projectFile);
                }
            }
            catch (Exception e)
            {
                Error.WriteLine(e.Message);
            }

        }
    }
}
