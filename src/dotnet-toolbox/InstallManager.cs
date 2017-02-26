using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DotNetToolbox.Helpers;
using Newtonsoft.Json.Linq;

namespace DotNetToolbox
{
    public class InstallManager
    {

        private ToolboxConfiguration _toolboxConfig;
        public InstallManager(ToolboxConfiguration tbc)
        {
            _toolboxConfig = tbc;
        }

        public void InstallTool(PackageMetadata pkg)
        {
        }

        #region RemoveToolCode

        public void UninstallTool(PackageMetadata pkg)
        {
            // Here we need to...
            // 1. Get the binary name based on the package metadata
            // 2. Get the scrpt name based on the OS
            // 3. Remove the script name
            // 4. Remove the version from the version file

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
                //this.Die($"There was an error installing the tool. The error returned was: {e.Message}");
            }

        }

        

        #endregion  


    }
}
