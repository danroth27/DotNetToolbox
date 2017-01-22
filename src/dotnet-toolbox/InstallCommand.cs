using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace DotNetToolbox
{
    public class InstallCommand : CommandLineApplication
    {
        private string _projectFile = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netcoreapp1.1</TargetFramework>
    <RootNamespace>DotNetToolbox</RootNamespace>
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
            var homeDir = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? Environment.GetEnvironmentVariable("USERPROFILE") : Environment.GetEnvironmentVariable("HOME");
            var dotnet = Path.Combine(homeDir, ".dotnet");
            var projectFile = Path.Combine(dotnet, "globalTools.csproj");
            try
            {
                if (!Directory.Exists(dotnet))
                {
                    Directory.CreateDirectory(dotnet);
                }
                if (!File.Exists(dotnet))
                {
                    File.WriteAllText(projectFile, _projectFile);
                }
            }
            catch (Exception e)
            {
                Error.WriteLine(e.Message);
            }
            Out.WriteLine($"I wish I could install {packageId}, but I don't know how! :'(");
            Out.WriteLine($"The home dir is {homeDir}");

            return 0;

        }
    }
}
