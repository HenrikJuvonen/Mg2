using System.Collections.Generic;
using System.Linq;
using MgKit.Interface;

namespace MgKit
{
    public class Package : IPackage
    {
        private readonly List<IPackage> _dependencies;
        private readonly List<IPackage> _dependents;
        private readonly List<IPackage> _allVersions; 
        private readonly List<string> _tags;
        private readonly List<string> _flags;
        private readonly List<string> _authors;

        private readonly object _flagsLock = new object();

        public Package(string id, string name, string version, string architecture, 
                       string description, string summary,
                       List<IPackage> dependencies, List<IPackage> dependents, List<IPackage> allVersions,
                       List<string> tags, List<string> authors,
                       List<string> flags)
        {
            Id = id;
            Name = name;
            Version = version;
            Architecture = architecture;

            Description = description;
            Summary = summary;

            _dependencies = dependencies;
            _dependents = dependents;
            _allVersions = allVersions;

            _tags = tags;
            _authors = authors;

            _flags = flags;
            _allVersions = allVersions;

            Files = new IFile[0];
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Version { get; private set; }
        public string Architecture { get; private set; }
        public string Description { get; private set; }
        public string Summary { get; private set; }
        public IEnumerable<IPackage> Dependencies { get { return _dependencies.ToArray(); } }
        public IEnumerable<IPackage> Dependents { get { return _dependents.ToArray(); } }
        public IEnumerable<IPackage> AllVersions { get { return _allVersions.ToArray(); } }
        public IEnumerable<string> Tags { get { return _tags.ToArray(); } }
        public IEnumerable<string> Flags
        {
            get
            {
                lock (_flagsLock)
                {
                    return _flags.ToArray();
                }
            }
        }
        public IEnumerable<string> Authors { get { return _authors.ToArray(); } }
        public IEnumerable<IFile> Files { get; private set; }

        public void AddFlags(params string[] flags)
        {
            lock (_flagsLock)
            {
                _flags.AddRange(flags);
            }
        }

        public void RemoveFlags(params string[] flags)
        {
            lock (_flagsLock)
            {
                _flags.RemoveAll(flags.Contains);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", Name, Version);
        }

        public void AddAllVersions(IEnumerable<IPackage> packages)
        {
            _allVersions.AddRange(packages);
        }
    }
}