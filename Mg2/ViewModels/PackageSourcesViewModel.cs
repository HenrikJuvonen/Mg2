using GalaSoft.MvvmLight.Command;
using Mg2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using MgKit;
using MgKit.Interface;

namespace Mg2.ViewModels
{
    public class PackageSourcesViewModel : Screen
    {
        private readonly Options _options;
        private IEnumerable<IPackageSource> _packageSources;
        private IPackageSource _selectedSource;
        private string _name;
        private string _location;
        private string _provider;

        public IEnumerable<IPackageSource> PackageSources
        {
            get { return _packageSources; }
            set { _packageSources = value; RaisePropertyChanged(); }
        }

        public IPackageSource SelectedSource
        {
            get { return _selectedSource; }
            set
            {
                _selectedSource = value;

                Name = _selectedSource != null ? _selectedSource.Name : null;
                Location = _selectedSource != null ? _selectedSource.Location : null;
                Provider = _selectedSource != null ? _selectedSource.Provider : null;
                RaisePropertyChanged();
            }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(); }
        }

        public string Location
        {
            get { return _location; }
            set { _location = value; RaisePropertyChanged(); }
        }

        public string Provider
        {
            get { return _provider; }
            set { _provider = value; RaisePropertyChanged(); }
        }

        public ICommand AddCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand UpdateCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        public PackageSourcesViewModel(Options options)
        {
            DisplayName = "Package Sources";
            _options = options;
            PackageSources = _options.PackageSources;
            AddCommand = new RelayCommand(Add);
            RemoveCommand = new RelayCommand(Remove, () => SelectedSource != null);
            UpdateCommand = new RelayCommand(Update, () =>
            {
                var x = SelectedSource != null && !PackageSources.Any(n => n.Name == Name && n.Location == Location && n.Provider == Provider);
                return x;
            });
            CloseCommand = new RelayCommand(TryClose);
        }

        private void Add()
        {
            _options.AddPackageSource();
            PackageSources = _options.PackageSources;
        }

        private void Remove()
        {
            _options.RemovePackageSource(SelectedSource);
            PackageSources = _options.PackageSources;
        }

        private void Update()
        {
            var sources = PackageSources.Except(new[] { SelectedSource }).Concat(new[] { new PackageSource(Location, Name, Provider) });
            _options.UpdatePackageSources(sources);
            PackageSources = _options.PackageSources;
        }
    }
}