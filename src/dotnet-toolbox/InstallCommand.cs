using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using Newtonsoft.Json.Linq;
using DotNetToolbox.DepsTools;
using DotNetToolbox.Helpers;
using DotNetToolbox.VersionMetadata;

namespace DotNetToolbox
{
    public class InstallCommand : CommandLineApplication
    {
        private ToolboxConfiguration _toolboxConfig;

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
            _toolboxConfig = new ToolboxConfiguration();
        }

        public PackageArgument PackageArgument { get; set; }
        public PackageVersionOption PackageVersionOption { get; set; }
        public SourceOption SourceOption { get; set; }

        public async Task<int> Run()
        {
            var pkgMetadata = new PackageMetadata(PackageArgument, PackageVersionOption);
            Out.WriteLine($"Installing {pkgMetadata.PackageId} {pkgMetadata.RequestedVersion}");


            EnsureToolboxDirExists();

            // Create temp project to restore the tool
            var restoreTargetFramework = GetTargetFrameworkForRestore();
            File.WriteAllText(_toolboxConfig.TempProjectPath, 
                              String.Format(_toolboxConfig.DefaultProject, 
                                            restoreTargetFramework, 
                                            pkgMetadata.PackageId, 
                                            pkgMetadata.RequestedVersion));

            Out.WriteLine("Restoring tool packages...");
            ExternalCommand.Create("dotnet", "restore", _toolboxConfig.TempProjectPath).Execute().EnsureSuccessful();


            pkgMetadata.RestoredVersion = GetRestoredPackageVersion(_toolboxConfig.TempProjectPath, pkgMetadata.PackageId);

            // We have the package restored, find out the dotnet-*.dll and create needed paths
            var targetFramework = GetTargetFramework(_toolboxConfig.NugetPackageRoot, pkgMetadata);

            var toolRootPath = Path.Combine(_toolboxConfig.NugetPackageRoot, pkgMetadata.PackageId, pkgMetadata.RestoredVersion, "lib", targetFramework);
            var toolBinaryPath = Directory.GetFiles(toolRootPath, "dotnet-*.dll").FirstOrDefault();
            var toolFileName = Path.GetFileNameWithoutExtension(toolBinaryPath);
            if (string.IsNullOrEmpty(toolBinaryPath))
            {
                this.Die("The tool package does not contain an assembly name the correct way.");

            }
            pkgMetadata.BinaryName = toolFileName;
            var toolsFolder = Path.Combine(_toolboxConfig.NugetPackageRoot, ".tools", pkgMetadata.PackageId, pkgMetadata.RestoredVersion, targetFramework);
            Out.WriteLine("Generating the runtime files for the tool...");
            var toolDepsFile = new DepsJsonBuilder().GenerateDepsFile(pkgMetadata, toolsFolder, toolFileName);

            var toolName = toolFileName.Substring(toolFileName.IndexOf('-') + 1);

            Out.WriteLine($"Adding {toolName} to the toolbox...");
            GenerateScriptForTool(_toolboxConfig.NugetPackageRoot, toolBinaryPath, toolDepsFile);
            WriteVersionToMetadata(pkgMetadata);
            Out.WriteLine($"Added {toolName} to the toolbox! Type 'dotnet {toolName}' to run the tool");
            return 0;
        }

        private string GetTargetFramework(string nugetPackagePath, PackageMetadata pkg)
        {
            var packageLibPath = Path.Combine(nugetPackagePath, pkg.PackageId.ToLower(), pkg.RestoredVersion, "lib");
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
            var assetsFilePath = Path.Combine(Path.GetDirectoryName(projectDir), "obj", "project.assets.json");
            var assetsFileText = File.ReadAllText(assetsFilePath);
            var assetsFileJson = JObject.Parse(assetsFileText);
            var packageIdWithVersion = assetsFileJson["libraries"]
                .Children<JProperty>()
                .First(lib => lib.Name.StartsWith($"{packageId}/", StringComparison.OrdinalIgnoreCase))
                .Name;
            var version = packageIdWithVersion.Split('/')[1];
            return version;
        }


        private void GenerateScriptForTool(string nugetPackagePath, string pathToTool, string pathToDepsFile)
        {
            var script = new StringBuilder();
            var scriptPath = Path.Combine(_toolboxConfig.ToolboxDirectoryPath, Path.GetFileNameWithoutExtension(pathToTool));
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
                    ExternalCommand.Create("chmod", "+x", scriptPath).CaptureStandardOut().Execute().EnsureSuccessful();
                }
            }
            catch (Exception e)
            {
                this.Die($"There was an error installing the tool. The error returned was: {e.Message}");
            }

        }

        private void EnsureToolboxDirExists()
        {
            if (!Directory.Exists(_toolboxConfig.ToolboxDirectoryPath))
            {
                this.Die("The toolbox directory does not exist; maybe you need to reinstall?");
            }

        }

        private void WriteVersionToMetadata(PackageMetadata pkg)
        {
            try
            {
                VersionFile.WriteVersion(pkg, _toolboxConfig.VersionsFile);
            }
            catch (Exception ex)
            {
                this.Die($"An error occured: {ex.Message}");
            }

        }
    }
}
