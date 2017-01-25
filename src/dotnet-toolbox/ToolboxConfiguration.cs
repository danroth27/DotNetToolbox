using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DotNetToolbox
{
    public class ToolboxConfiguration
    {
        public string ToolboxDirectoryPath { get; }
        public string ToolboxProjectPath { get; }
        public string HomeDirectory { get; }
        public string PackagesDir { get; }

        private const string _toolboxDirectory = ".toolbox";
        private const string _toolboxProject = "globaltools.csproj";

        public ToolboxConfiguration()
        {
            HomeDirectory = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? Environment.GetEnvironmentVariable("USERPROFILE") : Environment.GetEnvironmentVariable("HOME");
            ToolboxDirectoryPath = Path.Combine(HomeDirectory, _toolboxDirectory);
            ToolboxProjectPath = Path.Combine(ToolboxDirectoryPath, _toolboxProject);
            PackagesDir = "";
            
        }

    }
}
