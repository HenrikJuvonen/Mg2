using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using MgKit.Interface;

namespace MgKit.OneGet
{
    public class OneGetPackageSourceProvider : IPackageSourceProvider
    {
        private readonly RunspaceInvoke _ri = new RunspaceInvoke();

        private readonly ObservableCollection<IPackageSource> _packageSources = new ObservableCollection<IPackageSource>();

        public OneGetPackageSourceProvider()
        {
            Reload();
        }

        private bool _busy;

        public async void Reload()
        {
            _packageSources.Clear();

            dynamic results = null;

            await Task.Run(() =>
            {
                results = Get();
            });
            
            foreach (dynamic result in results)
            {
                _packageSources.Add(new PackageSource(result.Location, result.Name, result.Provider));
            }
        }

        public IEnumerable<IPackageSource> LoadPackageSources()
        {
            return _packageSources;
        }

        public async void SavePackageSources(IEnumerable<IPackageSource> packageSources)
        {
            if (_busy)
                return;

            _busy = true;

            await Task.Run(() =>
            {
                foreach (var packageSource in _packageSources.ToArray())
                {
                    if (packageSources.All(n => n != packageSource))
                    {
                        Remove(packageSource);
                    }
                }

                foreach (var packageSource in packageSources.ToArray())
                {
                    if (!_packageSources.Any(n => n.Name == packageSource.Name && n.Location == packageSource.Location && n.Provider == packageSource.Provider))
                    {
                        Add(packageSource);
                    }
                }
            });

            Reload();

            _busy = false;
        }

        private dynamic Get()
        {
            return _ri.Invoke("Get-PackageSource");
        }
        
        private void Add(IPackageSource packageSource)
        {
            _ri.Invoke("Add-PackageSource " + packageSource.Name + " " + packageSource.Location + " " + packageSource.Provider);
        }

        private void Remove(IPackageSource packageSource)
        {
            _ri.Invoke("Remove-PackageSource " + packageSource.Name + " " + packageSource.Provider);
        }
    }
}