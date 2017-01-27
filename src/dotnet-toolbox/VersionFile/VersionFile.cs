using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DotNetToolbox.VersionMetadata
{
    public class VersionFile
    {
        // I guess a dictionary here would work as well...
        public List<PackageMetadata> Contents { get; set; }
        private string _versionFilePath;

        public VersionFile(string path)
        {
            Contents = new List<PackageMetadata>();
            _versionFilePath = path;
            InitFile();
        }

        // Make sure that the file exists with something in it...should be called 
        // on constructor
        private void InitFile()
        {
            if (!File.Exists(_versionFilePath))
            {
                // create file here...
                throw new Exception("Stuffff");
            }
        }

        // Since it is JSON, just read the damn thing in PROVIDED IT WAS NOT ALREADY DONE
        // This will be if the list is empty (or at 0)
        private void ReadFile()
        {
            if (Contents.Count <= 0)
            {
                var file = File.ReadAllText("");
                Contents = JsonConvert.DeserializeObject<List<PackageMetadata>>(file);
            }
        }

        // Write it out by pushing 
        public void WriteVersion(PackageMetadata pkg)
        {
            
        }


    }
}
