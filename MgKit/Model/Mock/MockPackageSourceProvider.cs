using System.Collections.ObjectModel;
using System.Linq;
using MgKit.Model.Interface;
using System.Collections.Generic;

namespace MgKit.Model.Mock
{
    public class MockPackageSourceProvider : IPackageSourceProvider
    {
        private readonly IPackageSource _mockSource = new PackageSource("mock", "Mock", "Mock");

        private readonly ObservableCollection<IPackageSource> _packageSources;

        public MockPackageSourceProvider()
        {
            _packageSources = new ObservableCollection<IPackageSource> { _mockSource };
        }

        public IEnumerable<IPackageSource> LoadPackageSources()
        {
            return _packageSources;
        }

        public void SavePackageSources(IEnumerable<IPackageSource> packageSources)
        {
            packageSources = packageSources.ToArray();

            _packageSources.Clear();

            foreach (var packageSource in packageSources)
            {
                _packageSources.Add(packageSource);
            }

            if (_packageSources.All(n => n.Location != "mock"))
            {
                _packageSources.Add(_mockSource);
            }
        }
    }
}