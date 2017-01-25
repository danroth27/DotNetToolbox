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
using NuGet.Protocol.Core.Types;
using NuGet.Protocol;
using System.Threading;
using NuGet.Common;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyModel;
using NuGet.Frameworks;
using NuGet.ProjectModel;

namespace DotNetToolbox
{
    public class InstallCommand : CommandLineApplication
    {
        private const string _directory = ".toolbox";
        private ToolboxPaths _toolboxPaths;

        public InstallCommand(CommandLineApplication parent)
        {
            Parent = parent;
            Name = "install";
            PackageArgument = new PackageArgument(this);
            Arguments.Add(PackageArgument);
            PackageVersionOption = new PackageVersionOption(this);
            SourceOption = new SourceOption(this);
            OnExecute((Func<Task<int>>)Run);
            Parent.Commands.Add(this);
            HelpOption("-h|--help");
            _toolboxPaths = new ToolboxPaths();
        }

        public PackageArgument PackageArgument { get; set; }
        public PackageVersionOption PackageVersionOption { get; set; }
        public SourceOption SourceOption { get; set; }

        public async Task<int> Run()
        {
            var packageId = PackageArgument.Value;
            var packageVersion = PackageVersionOption.HasValue() ? PackageVersionOption.Value() : "*";
            Out.WriteLine($"Installing {packageId} {packageVersion}");

            // Get the paths
            //var paths = GetDirAndProjectPaths();
            EnsureToolboxDirectory();

            // Create temp project to restore the tool
            var restoreTargetFramework = GetTargetFrameworkForRestore();
            var tempProjectDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempProjectDir);
            var tempProjectPath = Path.Combine(tempProjectDir, "temp.csproj");
            var tempProject =
$@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>{restoreTargetFramework}</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""{packageId}"" Version=""{packageVersion}"" />
  </ItemGroup>
  <ItemGroup>
    <DotNetCliToolReference Include=""{packageId}"" Version=""{packageVersion}"" />
  </ItemGroup>
</Project>";
            Out.WriteLine(tempProject);
            File.WriteAllText(tempProjectPath, tempProject);
            Out.WriteLine("Restoring tool packages...");
            var restore = Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = SourceOption.HasValue() ? $"restore {tempProjectPath} -s {SourceOption.Value()}" : $"restore {tempProjectPath}",
                RedirectStandardOutput = false,
                RedirectStandardError = false
            });
            restore.WaitForExit();
            var restoredPackageVersion = GetRestoredPackageVersion(tempProjectDir, packageId);
            //Directory.Delete(tempProjectDir, recursive: true);

            // We have the package restored
            var nugetPackagePath = NuGetPathContext.Create(settingsRoot: Directory.GetCurrentDirectory()).UserPackageFolder
                // The package probing path option can't end with a slash, so we trim it here
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var targetFramework = GetTargetFramework(nugetPackagePath, packageId, restoredPackageVersion);
            var fullPath = Path.Combine(nugetPackagePath, packageId.ToLower(), restoredPackageVersion, "lib", targetFramework);
            //Out.WriteLine(fullPath);
            var toolPath = Directory.GetFiles(fullPath, "dotnet-*.dll").FirstOrDefault();
            var toolFileName = Path.GetFileNameWithoutExtension(toolPath);
            if (string.IsNullOrEmpty(toolPath))
            {
                throw new InvalidOperationException("The tool package does not contain a dotnet-*.dll file");
            }
            var toolsFolder = Path.Combine(nugetPackagePath, ".tools", packageId, restoredPackageVersion, targetFramework);
            Out.WriteLine("Generating the runtime files for the tool...");
            var blah = new DepsJsonBuilder().Build(
                new SingleProjectInfo(packageId, restoredPackageVersion, Enumerable.Empty<ResourceAssemblyInfo>()),
                null,
                new LockFileFormat().Read(Path.Combine(toolsFolder, "project.assets.json")),
                FrameworkConstants.CommonFrameworks.NetCoreApp10,
                null
                );
            var dcw = new DependencyContextWriter();
            var tempDepsFile = Path.GetTempFileName();
            var destDepsFile = Path.Combine(toolsFolder, $"{toolFileName}.deps.json");
            using (var file = File.Open(tempDepsFile, FileMode.OpenOrCreate, FileAccess.Write))
            {
                dcw.Write(blah, file);
            }
            try
            {
                File.Move(tempDepsFile, destDepsFile);
            }
            catch
            {
                // Error.WriteLine(e.Message);
                throw;
            }

            // return 0;

            // Find the dotnet-<foo> File

            var toolName = toolFileName.Substring(toolFileName.IndexOf('-') + 1);

            Out.WriteLine($"Adding {toolName} to the toolbox...");
            PutToolIntoPath(nugetPackagePath, toolPath, destDepsFile);
            Out.WriteLine($"Added {toolName} to the toolbox! Type 'dotnet {toolName}' to run the tool");
            return 0;
        }

        private string GetTargetFramework(string nugetPackagePath, string packageId, string packageVersion)
        {
            var packageLibPath = Path.Combine(nugetPackagePath, packageId.ToLower(), packageVersion, "lib");
            var toolTargets = Directory.GetDirectories(packageLibPath, "netcoreapp*");
            return toolTargets
                .Select(targetPath => new DirectoryInfo(targetPath).Name)
                .OrderByDescending(target => target.Remove(0, "netcoreapp".Length).Split('.')[0])
                .OrderByDescending(target => target.Remove(0, "netcoreapp".Length).Split('.')[1])
                .First();
        }

        private string GetTargetFrameworkForRestore()
        {
            // TODO: Figure out the latest installed shared framework, maybe consider tool targets as well
            // Hard code for now
            return "netcoreapp1.1";
        }

        private string GetRestoredPackageVersion(string projectDir, string packageId)
        {
            var assetsFilePath = Path.Combine(projectDir, "obj", "project.assets.json");
            var assetsFileText = File.ReadAllText(assetsFilePath);
            var assetsFileJson = JObject.Parse(assetsFileText);
            var packageIdWithVersion = assetsFileJson["libraries"]
                .Children<JProperty>()
                .First(lib => lib.Name.StartsWith($"{packageId}/"))
                .Name;
            var version = packageIdWithVersion.Split('/')[1];
            return version;
        }

        private async Task<string> ResolveLatestFromNuget(string packageId)
        {
            var repo = Repository.Factory.GetCoreV3(NuGetConstants.V3FeedUrl);
            var resource = repo.GetResource<MetadataResource>();
            var version = await resource.GetLatestVersion(packageId, true, false, NullLogger.Instance, CancellationToken.None);
            return version.ToString();
        }

        private void PutToolIntoPath(string nugetPackagePath, string pathToTool, string pathToDepsFile)
        {
            var script = new StringBuilder();
            var scriptPath = Path.Combine(_toolboxPaths.ToolboxDirectoryPath, Path.GetFileNameWithoutExtension(pathToTool));
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                script.AppendLine("@echo off");
                script.AppendLine($"dotnet exec --depsfile {pathToDepsFile} --additionalprobingpath {nugetPackagePath} {pathToTool} %*");
                scriptPath += ".cmd";
            }
            else
            {
                script.AppendLine("#!/bin/sh");
                script.AppendLine($"dotnet exec --depsfile {pathToDepsFile} --additionalprobingpath {nugetPackagePath} {pathToTool} \"$@\"");
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
        private void EnsureToolboxDirectory()
        {
            //var paths = GetDirAndProjectPaths();
            try
            {
                if (!Directory.Exists(_toolboxPaths.ToolboxDirectoryPath))
                {
                    Directory.CreateDirectory(_toolboxPaths.ToolboxDirectoryPath);
                }
            }
            catch (Exception e)
            {
                Error.WriteLine(e.Message);
            }

        }
    }
}
