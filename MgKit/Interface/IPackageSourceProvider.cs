using System.Collections.Generic;

namespace MgKit.Interface
{
    public interface IPackageSourceProvider
    {
        IEnumerable<IPackageSource> LoadPackageSources();
        void SavePackageSources(IEnumerable<IPackageSource> packageSources);
    }
}
