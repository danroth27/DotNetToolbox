using System;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using DotNetToolbox.Helpers;
using System.IO;

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
            var lines = File.ReadAllLines(_toolboxConfig.VersionsFile);

            if (lines.Length > 0)
            {

                foreach (var line in lines)
                {
                    var pair = line.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    Out.WriteLine($"    {pair[0]} ({pair[1]})");
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
