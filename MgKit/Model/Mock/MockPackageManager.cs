using MgKit.Model.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MgKit.Model.Mock
{
    public class MockPackageManager : IPackageManager
    {
        private const int MillisecondsDelay = 50;
        private const int ProgressStep = 10;

        private List<Package> _packages;

        public IEnumerable<IPackage> Packages { get { return _packages.ToArray(); } }

        public IPackageSourceProvider PackageSourceProvider { get; private set; }

        public Task Reload()
        {
            return Task.Run(() => {
                PackageSourceProvider = new MockPackageSourceProvider();

                _packages = new List<Package>
                {
                    new Package("test", "Test", "1.0", "any", "test..", "test.", new List<IPackage>(), new List<IPackage>(), new List<IPackage>(),new List<string> {"red"}, new List<string> {"author1", "author2"}, new List<string>()),
                    new Package("test", "Test", "2.0", "any", "test..", "test.", new List<IPackage>(), new List<IPackage>(), new List<IPackage>(),new List<string> {"red"}, new List<string> {"author1", "author2"}, new List<string> {"latest"}),
                    new Package("other", "Other", "1.1", "any", "other..", "other.", new List<IPackage>(), new List<IPackage>(), new List<IPackage>(),new List<string> {"red","blue"}, new List<string> {"author3"}, new List<string> {"installed", "updatable"}),
                    new Package("other", "Other", "1.2", "any", "other..", "other.", new List<IPackage>(), new List<IPackage>(), new List<IPackage>(),new List<string> {"red","blue"}, new List<string> {"author3"}, new List<string>()),
                    new Package("other", "Other", "1.3", "any", "other..", "other.", new List<IPackage>(), new List<IPackage>(), new List<IPackage>(),new List<string> {"red","blue"}, new List<string> {"author3"}, new List<string> {"latest"})
                };

                foreach (var package in _packages)
                {
                    var id = package.Id;
                    var arch = package.Architecture;
                    var allVersions = _packages.Where(n => n.Id == id && n.Architecture == arch).ToList();
                    package.AddAllVersions(allVersions);
                }
            });
        }

        public MockPackageManager()
        {
            Reload();
        }
        
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
                        case "fetch":
                            Fetch(action);
                            break;
                        case "install":
                            Install(action);
                            break;
                        case "reinstall":
                            Reinstall(action);
                            break;
                        case "update":
                            Update(action);
                            break;
                        case "uninstall":
                            Uninstall(action);
                            break;
                        case "lock":
                            Lock(action);
                            break;
                        case "unlock":
                            Unlock(action);
                            break;
                    }
                });
                SetFlags();
            });
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

        private readonly object _fetchLock = new object();

        private bool ShouldFetch(IPackage package)
        {
            lock (_fetchLock)
            {
                return package.Flags.All(n => n != "fetched");
            }
        }

        private void Fetch(PackageAction action)
        {
            lock (_fetchLock)
            {
                var package = (Package)action.Package;

                if (package.Flags.Any(n => n == "fetched"))
                {
                    action.Progress = 100;
                    action.Status = "fetched";
                    return;
                }

                action.Status = "fetching";
                SimulateProgressUpdate(action);
                action.Status = "fetched";
                package.AddFlags("fetched");
            }
        }

        private readonly object _installLock = new object();

        private bool ShouldInstall(IPackage package)
        {
            lock (_installLock)
            {
                return package.Flags.All(n => n != "installed");
            }
        }

        private void Install(PackageAction action)
        {
            lock (_installLock)
            {
                var package = (Package) action.Package;

                if (ShouldFetch(package))
                {
                    action.AddAction(new PackageAction(package, "fetch"));
                    CreateTask(action.Actions).RunSynchronously();
                }

                action.Status = "installing";
                SimulateProgressUpdate(action);
                action.Status = "installed";
                package.AddFlags("installed");
            }
        }

        private void Reinstall(PackageAction action)
        {
            var package = (Package)action.Package;

            if (ShouldFetch(package))
            {
                action.AddAction(new PackageAction(package, "fetch"));
            }
            action.AddAction(new PackageAction(package, "uninstall"));
            action.AddAction(new PackageAction(package, "install"));

            CreateTask(action.Actions).RunSynchronously();
        }

        private void Update(PackageAction action)
        {
            var package = (Package)action.Package;

            if (package.Flags.All(n => n != "installed"))
            {
                action.Progress = 100;
                action.Status = "failed";
                return;
            }

            var latest =
                _packages.SingleOrDefault(
                    n => n.Id == package.Id && n.Flags.All(m => m != "installed") && n.Flags.Any(m => m == "latest"));

            if (latest == null)
            {
                action.Progress = 100;
                action.Status = "failed";
                return;
            }

            if (ShouldFetch(latest))
            {
                action.AddAction(new PackageAction(latest, "fetch"));
            }
            if (ShouldInstall(latest))
            {
                action.AddAction(new PackageAction(latest, "install"));
            }

            if (!action.Actions.Any())
            {
                action.Progress = 100;
                action.Status = "failed";
                return;
            }

            action.IsIndeterminate = true;
            action.Status = "updating";

            CreateTask(action.Actions).RunSynchronously();

            action.Progress = 100;
            action.Status = "updated";
            package.RemoveFlags("updatable");
        }

        private void Uninstall(PackageAction action)
        {
            var package = (Package)action.Package;

            if (package.Flags.All(n => n != "installed"))
            {
                action.Progress = 100;
                action.Status = "failed";
                return;
            }

            action.Status = "uninstalling";
            SimulateProgressUpdate(action);
            action.Status = "uninstalled";
            package.RemoveFlags("installed");
        }

        private static void SimulateProgressUpdate(PackageAction action)
        {
            for (var i = 0; i < 100; i += ProgressStep)
            {
                action.Progress += ProgressStep;
                Task.Delay(MillisecondsDelay).Wait();
            }
        }

        private void Lock(PackageAction action)
        {
            var package = (Package)action.Package;

            action.Status = "locking";
            action.Progress = 100;
            action.Status = "locked";
            package.AddFlags("locked");
        }

        private void Unlock(PackageAction action)
        {
            var package = (Package)action.Package;

            action.Status = "unlocking";
            action.Progress = 100;
            action.Status = "unlocked";
            package.RemoveFlags("locked");
        }
    }
}