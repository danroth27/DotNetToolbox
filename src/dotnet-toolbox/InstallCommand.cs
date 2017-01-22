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

namespace DotNetToolbox
{
    public class InstallCommand : CommandLineApplication
    {
        private string _projectFile = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netcoreapp1.0</TargetFramework>
  </PropertyGroup>
</Project>";
        private string _shellScript = @"";
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
            var packageVersion = VersionOption.Value();
            Out.WriteLine($"The package ID provided was {packageId}");
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
            // Out.WriteLine(nugetPackagePath);
            // return 0;
            var pi = Process.Start(new ProcessStartInfo {
                FileName = "dotnet",
                Arguments = $"add package --version {packageVersion} {packageId}",
                WorkingDirectory = dotnet,
                UseShellExecute = false
            });
            pi.WaitForExit();
            if (pi.ExitCode != 0)
            {
                Error.WriteLine("The call to package failed");
                Error.WriteLine(pi.StandardError.ReadToEnd());
            }
            // We have the package restored
            var nugetPackagePath = NuGetPathContext.Create(settingsRoot: String.Empty).UserPackageFolder;
            var fullPath = Path.Combine(nugetPackagePath, packageId, packageVersion, "lib", "netcoreapp1.0");
            Out.WriteLine(fullPath);

            // Find the dotnet-<foo> File
            // foreach (var file in Directory.GetFiles(fullPath, "dotnet*.dll"))
            // {
            //     Out.WriteLine(file);
            // }
            var pathToTool = Directory.GetFiles(fullPath, "dotnet*.dll").FirstOrDefault();
            if (string.IsNullOrEmpty(pathToTool))
                throw new Exception("The package does not contain a dotnet-*.dll file");

            // Now we have the thing and we need to execute it using dotnet to make sure it works
            var p2 = Process.Start(new ProcessStartInfo {
                FileName = "dotnet",
                Arguments = $"--additionalprobingpath {nugetPackagePath} {fullPath}",
            });
            p2.WaitForExit();

            // Out.WriteLine($"I wish I could install {packageId}, but I don't know how! :'(");
            // Out.WriteLine($"The home dir is {homeDir}");

            return 0;

        }
    }
}
