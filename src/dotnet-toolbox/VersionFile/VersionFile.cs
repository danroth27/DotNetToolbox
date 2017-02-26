using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace DotNetToolbox.VersionMetadata
{
    public class VersionFile
    {

        public static void WriteVersion(PackageMetadata pkg, string versionFilePath)
        {
            var _contents = new List<PackageMetadata>();
            if (File.Exists(versionFilePath))
            {
                _contents = JsonConvert.DeserializeObject<List<PackageMetadata>>(File.ReadAllText(versionFilePath));
            }
            var existingVersion = _contents.SingleOrDefault(p => p.PackageId == pkg.PackageId);
            if (existingVersion != null)
            {
                _contents.Remove(existingVersion);
            }
            _contents.Add(pkg);
            WriteFile(versionFilePath, _contents);
            //File.WriteAllText(versionFilePath, JsonConvert.SerializeObject(_contents));
        }

        public static void RemoveVersion(PackageMetadata pkg, string versionFilePath)
        {
            var _contents = new List<PackageMetadata>();
            if (!File.Exists(versionFilePath))
                throw new InvalidOperationException("WTF??");

            var stuff = JsonConvert.DeserializeObject<List<PackageMetadata>>(File.ReadAllText(versionFilePath));
            var newStuff = stuff.Where(p => p.PackageId == pkg.PackageId).ToList();
            WriteFile(versionFilePath, newStuff);
            //File.WriteAllText(versionFilePath, JsonConvert.SerializeObject(_contents));
        }

        private static void WriteFile(string path, List<PackageMetadata> contents)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(contents));
        }


    }
}
