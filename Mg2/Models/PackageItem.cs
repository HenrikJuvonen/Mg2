using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using MgKit.Interface;

namespace Mg2.Models
{
    public class PackageItem : ObservableObject
    {
        private readonly IPackage _packageIdentity;

        public PackageItem(IPackage package)
        {
            _packageIdentity = package;
            
            Refresh();
        }

        protected bool Equals(PackageItem other)
        {
            return string.Equals(PackageIdentity, other.PackageIdentity);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((PackageItem) obj);
        }

        public override int GetHashCode()
        {
            return PackageIdentity.ToString().GetHashCode();
        }

        public void Refresh()
        {
            RaisePropertyChanged(() => IsInstalled);
            RaisePropertyChanged(() => IsLatest);
            RaisePropertyChanged(() => Status);
            RaisePropertyChanged(() => Mark);
        }
        
        public IPackage PackageIdentity
        {
            get { return _packageIdentity; }
        }

        public string Id
        {
            get { return _packageIdentity.Id; }
        }

        public string Name
        {
            get { return string.IsNullOrEmpty(_packageIdentity.Name) ? _packageIdentity.Id : _packageIdentity.Name; }
        }

        public string Version
        {
            get { return _packageIdentity.Version; }
        }

        public string Description
        {
            get { return _packageIdentity.Description; }
        }

        public string Summary
        {
            get
            {
                return String.IsNullOrEmpty(_packageIdentity.Summary)
                    ? (String.IsNullOrEmpty(_packageIdentity.Description)
                    ? string.Empty
                    : _packageIdentity.Description.Replace("\r\n", "").Replace("\n", ""))
                    : _packageIdentity.Summary.Replace("\r\n", "").Replace("\n", "");
            }
        }

        public IEnumerable<string> Tags
        {
            get { return _packageIdentity.Tags; }
        }
        
        public IEnumerable<string> Authors
        {
            get { return _packageIdentity.Authors; }
        }

        public bool IsInstalled
        {
            get { return PackageIdentity.Flags.Any(n => n == "installed"); }
        }

        public bool IsLatest
        {
            get { return PackageIdentity.Flags.Any(n => n == "latest"); }
        }
        
        public PackageMark Status
        {
            get
            {
                PackageMark status;

                if (PackageIdentity == null)
                    return PackageMark.Unmarked;

                if (IsInstalled)
                {
                    status = PackageIdentity.Flags.Any(n => n == "updatable") ? PackageMark.InstalledUpdatable : PackageMark.Installed;
                }
                else
                {
                    status = PackageIdentity.Flags.Any(n => n == "new") ? PackageMark.NotInstalledNew : PackageMark.NotInstalled;
                }

                return status;
            }
        }

        private PackageMark _mark = PackageMark.Unmarked;

        public PackageMark Mark
        {
            get
            {
                return _mark == PackageMark.Unmarked ? Status : _mark;
            }
            set
            {
                if (!CanBeMarked(value))
                    return;

                _mark = value;
                RaisePropertyChanged(() => Mark);
            }
        }

        public bool IsUnmarked
        {
            get
            {
                return _mark == PackageMark.Unmarked;
            }
        }

        public bool CanBeMarked(PackageMark mark)
        {
            if (Status == PackageMark.Unmarked)
                return true;

            if (_mark == mark)
                return false;

            if (_mark != PackageMark.Unmarked && mark != PackageMark.Unmarked)
                return false;

            switch (mark)
            {
                case PackageMark.MarkedForUpdate:
                    return Status == PackageMark.InstalledUpdatable;
                case PackageMark.MarkedForInstallation:
                    return !(Status != PackageMark.NotInstalled &&
                             Status != PackageMark.NotInstalledNew);
                case PackageMark.MarkedForReinstallation:
                case PackageMark.MarkedForRemoval:
                    return !(Status != PackageMark.Installed &&
                             Status != PackageMark.InstalledUpdatable &&
                             Status != PackageMark.Broken);
            }
            return true;
        }

        public override string ToString()
        {
            return Name + " [" + Version + "]";
        }
    }
}