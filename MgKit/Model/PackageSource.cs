using MgKit.Model.Interface;

namespace MgKit.Model
{
    public class PackageSource : IPackageSource
    {
        public PackageSource(string location, string name, string provider)
        {
            Location = location;
            Name = name;
            Provider = provider;
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}] ({2})", Name, Location, Provider);
        }

        public string Location { get; set; }
        public string Name { get; set; }
        public string Provider { get; set; }
    }
}