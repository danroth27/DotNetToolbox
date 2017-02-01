using System;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using DotNetToolbox.Helpers;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DotNetToolbox
{
    public class ListCommand : CommandLineApplication
    {
        private ToolboxConfiguration _toolboxConfig;

        public ListCommand(CommandLineApplication parent)
        {
            Parent = parent;
            Name = "list";
            OnExecute((Func<Task<int>>)Run);
            Parent.Commands.Add(this);
            HelpOption("-h|--help");
            _toolboxConfig = new ToolboxConfiguration();
        }

        public async Task<int> Run()
        {
            if (!File.Exists(_toolboxConfig.VersionsFile))
            {
                this.Die("The versions file does not exist.");
            }
            Out.WriteLine("Installed packages on this machine:");
            var lines = JsonConvert.DeserializeObject<List<PackageMetadata>>(File.ReadAllText(_toolboxConfig.VersionsFile));
            if (lines.Count > 0)
            {

                foreach (var line in lines)
                {
                    Out.WriteLine($"\t{line.PackageId} ({line.RestoredVersion})");
                }
            }
            else
            {
                Out.WriteLine("There are no packages installed on this machine it seems. Run dotnet toolbox install --help.");
            }

            return 0;
        }
    }
}
