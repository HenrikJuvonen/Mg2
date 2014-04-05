using System;
using System.Linq;
using MgKit.Model.Interface;
using Xunit;

namespace MgKit.Model.Mock
{
    public class MockPackageManagerTest
    {
        public IPackageManager PackageManager = new MockPackageManager();

        public void TryFetch(IPackage package)
        {
            if (package.Flags.Any(n => n == "fetched"))
                return;

            PackageManager.Execute(new PackageAction(package, "fetch")).Wait();
        }

        public void TryInstall(IPackage package)
        {
            if (package.Flags.Any(n => n == "installed"))
                return;

            PackageManager.Execute(new PackageAction(package, "install")).Wait();
        }

        private void TryReinstall(IPackage package)
        {
            if (package.Flags.All(n => n != "installed"))
                throw new Exception("package not installed");

            PackageManager.Execute(new PackageAction(package, "reinstall")).Wait();
        }

        private void TryUninstall(IPackage package)
        {
            if (package.Flags.All(n => n != "installed"))
                throw new Exception("package not installed");

            PackageManager.Execute(new PackageAction(package, "uninstall")).Wait();
        }

        private void TryLock(IPackage package)
        {
            PackageManager.Execute(new PackageAction(package, "lock")).Wait();
        }

        private void TryUnlock(IPackage package)
        {
            PackageManager.Execute(new PackageAction(package, "unlock")).Wait();
        }

        [Fact]
        public void PackageIsListed()
        {
            Assert.True(PackageManager.Packages.Any(n => n.Id == "test"));
        }

        [Fact]
        public void PackageGetsFetched()
        {
            var package = PackageManager.Packages.Single(n => n.Id == "test");

            TryFetch(package);
            Assert.True(package.Flags.Any(n => n == "fetched"));
        }

        [Fact]
        public void PackageGetsInstalled()
        {
            var package = PackageManager.Packages.Single(n => n.Id == "test");

            TryFetch(package);
            Assert.True(package.Flags.Any(n => n == "fetched"));

            TryInstall(package);
            Assert.True(package.Flags.Any(n => n == "installed"));
        }

        [Fact]
        public void PackageGetsReinstalled()
        {
            var package = PackageManager.Packages.Single(n => n.Id == "test");

            TryFetch(package);
            Assert.True(package.Flags.Any(n => n == "fetched"));

            TryInstall(package);
            Assert.True(package.Flags.Any(n => n == "installed"));

            TryReinstall(package);
            Assert.True(package.Flags.Any(n => n == "installed"));
        }

        [Fact]
        public void PackageGetsUninstalled()
        {
            var package = PackageManager.Packages.Single(n => n.Id == "test");

            TryFetch(package);
            Assert.True(package.Flags.Any(n => n == "fetched"));

            TryInstall(package);
            Assert.True(package.Flags.Any(n => n == "installed"));

            TryUninstall(package);
            Assert.False(package.Flags.Any(n => n == "installed"));
        }

        [Fact]
        public void PackageGetsLocked()
        {
            var package = PackageManager.Packages.Single(n => n.Id == "test");

            TryLock(package);
            Assert.True(package.Flags.Any(n => n == "locked"));
        }

        [Fact]
        public void PackageGetsUnlocked()
        {
            var package = PackageManager.Packages.Single(n => n.Id == "test");

            TryUnlock(package);
            Assert.False(package.Flags.Any(n => n == "locked"));
        }
    }
}