using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using MgKit.Model.Interface;

namespace MgKit.Model.OneGet
{
    public class OneGetPackageManager : IPackageManager
    {
        private readonly RunspaceInvoke _ri = new RunspaceInvoke();

        private readonly List<Package> _packages = new List<Package>(); 

        public OneGetPackageManager()
        {
            PackageSourceProvider = new OneGetPackageSourceProvider();
        }

        public async Task Reload()
        {
            _packages.Clear();

            await Task.Run(() =>
            {
                lock (_riLock)
                {
                    dynamic find = _ri.Invoke("Find-Package");

                    foreach (var result in find)
                    {
                        var pkg = new Package(result.Name, result.Name, result.Version, string.Empty, result.Summary,
                            result.Summary, new List<IPackage>(), new List<IPackage>(), new List<IPackage>(),
                            new List<string>(), new List<string>(), new List<string>());
                        _packages.Add(pkg);
                    }

                    dynamic get = _ri.Invoke("Get-Package");

                    foreach (var result in get)
                    {
                        var pkg = new Package(result.Name, result.Name, result.Version, string.Empty, result.Summary,
                            result.Summary, new List<IPackage>(), new List<IPackage>(), new List<IPackage>(),
                            new List<string>(), new List<string>(), new List<string>());
                        if (result.Status == "Installed")
                        {
                            pkg.AddFlags("installed");
                        }
                        var ex = _packages.FirstOrDefault(n => n.Name == pkg.Name && n.Version == pkg.Version);

                        if (ex != null && result.Status == "Installed")
                            ex.AddFlags("installed");
                        else
                            _packages.Add(pkg);
                    }

                    foreach (var package in _packages)
                    {
                        var id = package.Id;
                        var allVersions = Packages.Where(n => n.Id == id).ToList();
                        package.AddAllVersions(allVersions);
                    }
                }
            });
        }

        public IEnumerable<IPackage> Packages { get { return _packages; }}
        public IPackageSourceProvider PackageSourceProvider { get; private set; }

        public Task Execute(params PackageAction[] actions)
        {
            var t = CreateTask(PackageAction.OrderActionsByType(actions));
            t.Start();
            return t;
        }

        private Task CreateTask(IEnumerable<PackageAction> actions)
        {
            return new Task(() =>
            {
                Parallel.ForEach(actions, action =>
                {
                    switch (action.Type)
                    {
                        case "install":
                            Install(action);
                            break;
                        case "uninstall":
                            Uninstall(action);
                            break;
                    }
                });
                SetFlags();
            });
        }

        private readonly object _riLock = new object();

        private void Install(PackageAction action)
        {
            lock (_riLock)
            {
                var package = (Package)action.Package;

                action.Status = "installing";
                IList err;
                dynamic res = _ri.Invoke("Install-Package $input -Force", new[] { package.Name }, out err);
                action.Status = "installed";
                action.Progress = 100;

                foreach (var pkg in res)
                {
                    var ex = _packages.FirstOrDefault(n => n.Name == pkg.Name && n.Version == pkg.Version);
                    if (ex != null && pkg.Status == "Installed")
                    {
                        ex.AddFlags("installed");
                    }
                }
            }
        }

        private void Uninstall(PackageAction action)
        {
            lock (_riLock)
            {
                var package = (Package)action.Package;

                if (package.Flags.All(n => n != "installed"))
                {
                    action.Progress = 100;
                    action.Status = "failed";
                    return;
                }

                action.Status = "uninstalling";
                IList err;
                dynamic res = _ri.Invoke("Uninstall-Package $input -Force", new[] { package.Name }, out err);
                action.Status = "uninstalled";
                action.Progress = 100;
                foreach (var pkg in res)
                {
                    var ex = _packages.FirstOrDefault(n => n.Name == pkg.Name && n.Version == pkg.Version);
                    if (ex != null && pkg.Status != "Installed")
                    {
                        ex.RemoveFlags("installed");
                    }
                }
            }
        }

        private void SetFlags()
        {
            foreach (var package in _packages)
            {
                SetUpdatableFlag(package);
            }
        }

        private void SetUpdatableFlag(Package package)
        {
            var latest = _packages.SingleOrDefault(n => n.Id == package.Id && n.Flags.All(m => m != "installed") && n.Flags.Any(m => m == "latest"));

            if (latest != null)
            {
                package.AddFlags("updatable");
            }
            else
            {
                package.RemoveFlags("updatable");
            }
        }
    }
}