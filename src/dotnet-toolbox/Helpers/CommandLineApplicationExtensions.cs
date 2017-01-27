using System;
using Microsoft.Extensions.CommandLineUtils;

namespace DotNetToolbox.Helpers
{
    public static class CommandLineApplicationExtensions
    {
        public static void Die(this CommandLineApplication app, string message, int returnCode = 1)
        {
            app.Error.WriteLine("An error happened that caused the program to exit.");
            app.Error.WriteLine($"The erroris: {message}");
            Environment.Exit(returnCode);
        }
    }
}
