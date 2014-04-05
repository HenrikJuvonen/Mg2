using System.Collections.Generic;
using System.Threading.Tasks;

namespace MgKit.Model.Interface
{
    public interface IPackageManager
    {
        Task Reload();
        IEnumerable<IPackage> Packages { get; }
        IPackageSourceProvider PackageSourceProvider { get; }
        Task Execute(params PackageAction[] actions);
    }
}
