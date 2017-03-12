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
            if (!File.Exists(versionFilePath))
                throw new InvalidOperationException("The versions file does not exist.");

            var existingItems = JsonConvert.DeserializeObject<List<PackageMetadata>>(File.ReadAllText(versionFilePath));
            var newStuff = existingItems.Where(p => p.PackageId != pkg.PackageId).ToList();
            WriteFile(versionFilePath, newStuff);
        }

        public static PackageMetadata Get(string packageId, string path)
        {
            return ReadFile(path).SingleOrDefault(p => p.PackageId == packageId);
        }

        public static PackageMetadata Get(Func<PackageMetadata, bool> predicate, string path)
        {
            return ReadFile(path).SingleOrDefault(predicate);
        }

        private static IEnumerable<PackageMetadata> ReadFile(string path)
        {
            var contents = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<IEnumerable<PackageMetadata>>(contents);
        }

        private static void WriteFile(string path, List<PackageMetadata> contents)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(contents));
        }


    }
}
