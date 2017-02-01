using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace DotNetToolbox.VersionMetadata
{
    public class VersionFile
    {
        private static List<PackageMetadata> _contents = new List<PackageMetadata>();

        public static void WriteVersion(PackageMetadata pkg, string versionFilePath)
        {
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
            File.WriteAllText(versionFilePath, JsonConvert.SerializeObject(_contents));
        }


    }
}
