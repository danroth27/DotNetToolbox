namespace DotNetToolbox
{
    public class PackageMetadata
    {
        public string PackageId { get; set; }
        public string RequestedVersion { get; set; }
        public string RestoredVersion { get; set; }

        public PackageMetadata(string pid, string rv, string rev)
        {
            PackageId = pid;
            RequestedVersion = rv;
            RestoredVersion = rev;
        }

        public PackageMetadata(PackageArgument pa, PackageVersionOption pvo)
        {
            PackageId = pa.Value;
            RequestedVersion = pvo.HasValue() ? pvo.Value() : "*";
        }

        public PackageMetadata()
        { }
    }
}