using System.Collections.Generic;

namespace MgKit.Model.Interface
{
    public interface IPackageSourceProvider
    {
        IEnumerable<IPackageSource> LoadPackageSources();
        void SavePackageSources(IEnumerable<IPackageSource> packageSources);
    }
}
