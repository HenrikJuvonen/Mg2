using System.Collections.Generic;

namespace MgKit.Model.Interface
{
    public interface IPackage
    {
        string Id { get; }
        string Name { get; }
        string Version { get; }
        string Architecture { get; }
        IEnumerable<IPackage> Dependencies { get; }
        IEnumerable<IPackage> Dependents { get; }
        IEnumerable<IPackage> AllVersions { get; }
        IEnumerable<string> Tags { get; }
        IEnumerable<string> Flags { get; }
        IEnumerable<string> Authors { get; }
        string Description { get; }
        string Summary { get; }
        IEnumerable<IFile> Files { get; }
    }

    public interface IFile
    {
        string Path { get; }
    }
}