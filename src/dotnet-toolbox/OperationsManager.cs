using System;
namespace DotNetToolbox
{
    public class OperationsManager
    {
        public OperationsManager()
        {
        }

        public void InstallTool(PackageMetadata pkg)
        {
        }

        #region RemoveToolCode

        public void RemoveTool(PackageMetadata pkg)
        {
            // Here we need to...
            // 1. Get the binary name based on the package metadata
            // 2. Get the scrpt name based on the OS
            // 3. Remove the script name
            // 4. Remove the version from the version file
        }

        

        #endregion  


    }
}
