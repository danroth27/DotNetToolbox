using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using NuGet.Configuration;

namespace DotNetToolbox
{
    public class ToolboxConfiguration
    {
        public string ToolboxDirectoryPath { get; }
        public string ToolboxProjectPath { get; }
        public string HomeDirectory { get; }
        public string NugetPackageRoot { get; }
        public string VersionsFile { get; }
        public string DefaultProject = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>{0}</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""{1}"" Version=""{2}"" />
  </ItemGroup>
  <ItemGroup>
    <DotNetCliToolReference Include=""{1}"" Version=""{2}"" />
  </ItemGroup>
</Project>";
        public string TempProjectPath { get; }

        private const string _toolboxDirectory = ".toolbox";
        private const string _toolboxProject = "temp.csproj";

        public ToolboxConfiguration()
        {
            HomeDirectory = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? Environment.GetEnvironmentVariable("USERPROFILE") : Environment.GetEnvironmentVariable("HOME");
            ToolboxDirectoryPath = Path.Combine(HomeDirectory, _toolboxDirectory);
            ToolboxProjectPath = Path.Combine(ToolboxDirectoryPath, _toolboxProject);
            NugetPackageRoot = NuGetPathContext.Create(settingsRoot: Directory.GetCurrentDirectory()).UserPackageFolder
                // The package probing path option can't end with a slash, so we trim it here
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);;
            VersionsFile = Path.Combine(ToolboxDirectoryPath, "versions.json");
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            TempProjectPath = Path.Combine(tempDir, _toolboxProject);
        }

    }
}
