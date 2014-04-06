using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Mg2.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Xml.Serialization;
using MgKit;
using MgKit.Interface;

namespace Mg2.ViewModels
{
    public class MainViewModel : Screen
    {
        private bool _isIdle;
        private List<PackageItem> _packageItems;
        private PackageItem _selectedPackageItem;
        private Filter _selectedFilter;
        private string _query = string.Empty;
        private int _totalCount;
        private int _currentCount;

        public IWindowManager WindowManager { get; set; }
        
        public ICommand ReloadCommand { get; set; }
        public ICommand MarkCommand { get; set; }
        public ICommand MarkButtonClickCommand { get; set; }

        public ICommand ReadMarkingsCommand { get; set; }
        public ICommand SaveMarkingsAsCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        public ICommand UnmarkAllCommand { get; set; }
        public ICommand ApplyMarkedChangesCommand { get; set; }

        public ICommand UnmarkCommand { get; set; }
        public ICommand MarkForInstallationCommand { get; set; }
        public ICommand MarkForUpdateCommand { get; set; }
        public ICommand MarkForRemovalCommand { get; set; }
        public ICommand PropertiesCommand { get; set; }

        public ICommand OptionsCommand { get; set; }
        public ICommand RepositoriesCommand { get; set; }
        public ICommand FiltersCommand { get; set; }

        public ICommand ContentsCommand { get; set; }
        public ICommand AboutCommand { get; set; }
        
        public List<PackageItem> PackageItems
        {
            get { return _packageItems; }
            set { _packageItems = value; RaisePropertyChanged(); }
        }

        public bool IsIdle
        {
            get { return _isIdle; }
            set { _isIdle = value; RaisePropertyChanged(); }
        }

        public PackageItem SelectedPackageItem
        {
            get { return _selectedPackageItem; }
            set { _selectedPackageItem = value; RaisePropertyChanged(); RaisePropertyChanged(() => SelectedPackageItemDescription); }
        }

        public string SelectedPackageItemDescription
        {
            get { return _selectedPackageItem != null ? _selectedPackageItem.Description : null; }
        }

        public IPackageManager PackageManager { get; set; }

        public Options Options { get; set; }

        public ObservableCollection<Filter> Filters { get; set; }
        
        public Filter SelectedFilter
        {
            get
            {
                return _selectedFilter;
            }
            set
            {
                _selectedFilter = value;
                
                if (_selectedFilter != null)
                {
                    _query = _selectedFilter.Query;
                    RaisePropertyChanged(() => Query);
                }

                RaisePropertyChanged(() => SelectedFilter);
            }
        }

        public string Query
        {
            get
            {
                return _query;
            }
            set
            {
                _query = value;
                SelectedFilter = null;
                RaisePropertyChanged(() => Query);
            }
        }
        
        public MainViewModel()
        {
            DisplayName = "Mg2";
            WindowManager = SimpleIoc.Default.GetInstance<IWindowManager>();

            ReloadCommand = new RelayCommand(Reload);
            MarkCommand = new RelayCommand(() => TryMark());
            MarkButtonClickCommand = new RelayCommand<RoutedEventArgs>(OnMarkButtonClicked);

            ReadMarkingsCommand = new RelayCommand(ReadMarkings);
            SaveMarkingsAsCommand = new RelayCommand(SaveMarkingsAs);
            CloseCommand = new RelayCommand(TryClose);

            UnmarkAllCommand = new RelayCommand(UnmarkAll, () => PackageItems.Any(n => !n.IsUnmarked));
            ApplyMarkedChangesCommand = new RelayCommand(ApplyMarkedChanges, () => PackageItems.Any(n => !n.IsUnmarked));

            UnmarkCommand = new RelayCommand(Unmark, () => SelectedPackageItem != null && SelectedPackageItem.CanBeMarked(PackageMark.Unmarked));
            MarkForInstallationCommand = new RelayCommand(MarkForInstallation, () => SelectedPackageItem != null && SelectedPackageItem.CanBeMarked(PackageMark.MarkedForInstallation));
            MarkForUpdateCommand = new RelayCommand(MarkForUpdate, () => SelectedPackageItem != null && SelectedPackageItem.CanBeMarked(PackageMark.MarkedForUpdate));
            MarkForRemovalCommand = new RelayCommand(MarkForRemoval, () => SelectedPackageItem != null && SelectedPackageItem.CanBeMarked(PackageMark.MarkedForRemoval));
            PropertiesCommand = new RelayCommand(() => WindowManager.ShowDialog(new PropertiesViewModel(SelectedPackageItem)), () => SelectedPackageItem != null);

            OptionsCommand = new RelayCommand(() => WindowManager.ShowDialog(new OptionsViewModel(Options)));
            RepositoriesCommand = new RelayCommand(() => WindowManager.ShowDialog(new PackageSourcesViewModel(Options)));
            FiltersCommand = new RelayCommand(() => WindowManager.ShowDialog(new FiltersViewModel(Filters)));

            ContentsCommand = new RelayCommand(ShowHelpContents);
            AboutCommand = new RelayCommand(() => WindowManager.ShowDialog(new AboutViewModel()));

            PackageItems = new List<PackageItem>();

            IsIdle = true;

            PackageManager = SimpleIoc.Default.GetInstance<IPackageManager>();

            Options = new Options();
            Options.Load();

            InitializeFilters();

            Load();
        }
        
        private void InitializeFilters()
        {
            try
            {
                var path = Constants.AppDataPath + "Filters.xml";

                var reader = new XmlSerializer(typeof(ObservableCollection<Filter>), new XmlRootAttribute("Filters"));
                var file = new StreamReader(path);
                Filters = (ObservableCollection<Filter>)reader.Deserialize(file);
                file.Close();
            }
            catch
            {
                Filters = new ObservableCollection<Filter>(new[]
                {
                    new Filter("All", ""),
                    new Filter("Installed", "installed")
                });
            }
            SelectedFilter = Filters.FirstOrDefault();
        }

        private async void Load(bool forceDownload = false)
        {
            if (!IsIdle)
                return;

            IsIdle = false;
            
            PackageItems.Clear();

            Refresh();

            await PackageManager.Reload();
            PackageItems.AddRange(PackageManager.Packages.Select(n => new PackageItem(n)));

            _totalCount = 0;
            _currentCount = 0;

            IsIdle = true;

            Refresh();
        }

        private void Refresh()
        {
            SelectedFilter = null;
            SelectedFilter = Filters.FirstOrDefault() ?? new Filter();
            RaisePropertyChanged(() => Status);
        }

        private void Reload()
        {
            Load(true);
        }

        private void ReadMarkings()
        {
            throw new NotImplementedException();
        }

        private void SaveMarkingsAs()
        {
            throw new NotImplementedException();
        }

        private void UnmarkAll()
        {
            foreach (var packageItem in PackageItems)
            {
                packageItem.Mark = PackageMark.Unmarked;
            }
        }

        public void ApplyMarkedChanges()
        {
            var installed = PackageItems.Where(n => n.Mark == PackageMark.MarkedForInstallation).OrderBy(n => n.Name).ThenBy(n => n.Version);
            var updated = PackageItems.Where(n => n.Mark == PackageMark.MarkedForUpdate).OrderBy(n => n.Name).ThenBy(n => n.Version);
            var reinstalled = PackageItems.Where(n => n.Mark == PackageMark.MarkedForReinstallation).OrderBy(n => n.Name).ThenBy(n => n.Version);
            var removed = PackageItems.Where(n => n.Mark == PackageMark.MarkedForRemoval).OrderBy(n => n.Name).ThenBy(n => n.Version);

            if (WindowManager.ShowDialog(new SummaryViewModel(installed, updated, reinstalled, removed)) != true)
                return;

            var actions = new List<PackageAction>();
            actions.AddRange(installed.Select(packageItem => new PackageAction(packageItem.PackageIdentity, "install")));
            actions.AddRange(updated.Select(packageItem => new PackageAction(packageItem.PackageIdentity, "update")));
            actions.AddRange(reinstalled.Select(packageItem => new PackageAction(packageItem.PackageIdentity, "reinstall")));
            actions.AddRange(removed.Select(packageItem => new PackageAction(packageItem.PackageIdentity, "uninstall")));

            if (WindowManager.ShowDialog(new ActionsViewModel(PackageManager, actions)) != true)
            {
            }

            UnmarkAll();

            foreach (var packageItem in PackageItems)
            {
                packageItem.Refresh();
            }

            Refresh();
        }

        private bool TryMark()
        {
            if (SelectedPackageItem == null)
                return false;

            if (SelectedPackageItem.Status == PackageMark.NotInstalled || SelectedPackageItem.Status == PackageMark.NotInstalledNew)
            {
                if (SelectedPackageItem.IsUnmarked)
                    MarkForInstallation();
                else
                    Unmark();

                return true;
            }

            return false;
        }

        private void OnMarkButtonClicked(RoutedEventArgs e)
        {
            var element = e.Source as FrameworkElement;

            if (element != null)
            {
                SelectedPackageItem = (PackageItem)element.ToolTip;

                if (Options.ClickingOnTheStatusIconMarksTheMostLikelyAction && TryMark())
                    return;

                if (SelectedPackageItem == null)
                    return;

                if (element.IsMouseOver)
                {
                    element.ContextMenu.Placement = PlacementMode.MousePoint;
                    element.ContextMenu.HorizontalOffset = 0;
                    element.ContextMenu.VerticalOffset = 0;
                }
                else
                {
                    element.ContextMenu.Placement = PlacementMode.RelativePoint;
                    element.ContextMenu.HorizontalOffset = element.Width / 2;
                    element.ContextMenu.VerticalOffset = element.Height / 2;
                }
                element.ContextMenu.PlacementTarget = element;
                element.ContextMenu.IsOpen = true;
            }
        }

        private void Unmark()
        {
            switch (SelectedPackageItem.Mark)
            {
                case PackageMark.MarkedForInstallation:
                    {
                        var installed = GetDependants(SelectedPackageItem.PackageIdentity).Where(n => n.Mark == PackageMark.MarkedForInstallation).ToArray();

                        if (!MarkRelated(installed, unmark: true))
                            return;
                    }
                    break;
                case PackageMark.MarkedForUpdate:
                    {
                        var updated = GetDependants(SelectedPackageItem.PackageIdentity).Where(n => n.Mark == PackageMark.MarkedForUpdate).ToArray();

                        if (!MarkRelated(updated: updated, unmark: true))
                            return;
                    }
                    break;
                case PackageMark.MarkedForRemoval:
                    {
                        var removed = GetDependencies(SelectedPackageItem.PackageIdentity).Where(n => n.Mark == PackageMark.MarkedForRemoval).ToArray();

                        if (!MarkRelated(removed: removed, unmark: true))
                            return;
                    }
                    break;
            }

            SelectedPackageItem.Mark = PackageMark.Unmarked;

            RaisePropertyChanged(() => Status);
        }

        private void MarkForInstallation()
        {
            var installed = GetDependencies(SelectedPackageItem.PackageIdentity).Where(n => n.CanBeMarked(PackageMark.MarkedForInstallation)).ToArray();

            if (!MarkRelated(installed))
                return;

            SelectedPackageItem.Mark = PackageMark.MarkedForInstallation;

            RaisePropertyChanged(() => Status);
        }

        private void MarkForUpdate()
        {
            var updated = GetDependencies(SelectedPackageItem.PackageIdentity).Where(n => n.CanBeMarked(PackageMark.MarkedForUpdate)).ToArray();

            if (!MarkRelated(updated: updated))
                return;

            SelectedPackageItem.Mark = PackageMark.MarkedForUpdate;

            RaisePropertyChanged(() => Status);
        }

        private void MarkForRemoval()
        {
            var removed = GetDependants(SelectedPackageItem.PackageIdentity).Where(n => n.CanBeMarked(PackageMark.MarkedForRemoval)).ToArray();

            if (!MarkRelated(removed: removed))
                return;

            SelectedPackageItem.Mark = PackageMark.MarkedForRemoval;

            RaisePropertyChanged(() => Status);
        }

        private bool MarkRelated(PackageItem[] installed = null, PackageItem[] updated = null, PackageItem[] removed = null, bool unmark = false)
        {
            if ((installed != null && installed.Any()) || (updated != null && updated.Any()) || (removed != null && removed.Any()))
            {
                if (Options.AskToConfirmChangesThatAlsoAffectOtherPackages)
                {
                    var canProceed = WindowManager.ShowDialog(new MarkAdditionalChangesViewModel(installed, updated, removed));

                    if (canProceed != true)
                        return false;
                }

                if (installed != null) foreach (var p in installed)
                    p.Mark = unmark ? PackageMark.Unmarked : PackageMark.MarkedForInstallation;

                if (updated != null) foreach (var p in updated)
                    p.Mark = unmark ? PackageMark.Unmarked : PackageMark.MarkedForInstallation;

                if (removed != null) foreach (var p in removed)
                    p.Mark = unmark ? PackageMark.Unmarked : PackageMark.MarkedForRemoval;
            }
            return true;
        }

        private IEnumerable<PackageItem> GetDependencies(IPackage package)
        {
            var dependencies = package.Dependencies;
            return PackageItems.Where(n => dependencies.Contains(n.PackageIdentity));
        }

        private IEnumerable<PackageItem> GetDependants(IPackage package)
        {
            var dependents = package.Dependents;
            return PackageItems.Where(n => dependents.Contains(n.PackageIdentity));
        }

        private void ShowHelpContents()
        {
            //Process.Start(Constants.HelpContents);
        }

        private void UpdateLoadProgress(int n)
        {
            _currentCount += n;

            if (_currentCount > _totalCount)
                _totalCount = _currentCount;

            RaisePropertyChanged(() => Status);
        }

        public string Status
        {
            get
            {
                if (IsIdle)
                {
                    var total = PackageItems.Count();

                    var installed = PackageItems.Count(n =>
                        n.Status == PackageMark.Installed ||
                        n.Status == PackageMark.InstalledUpdatable);

                    var toinstall = PackageItems.Count(n =>
                        n.Mark == PackageMark.MarkedForInstallation);

                    var toupdate = PackageItems.Count(n =>
                        n.Mark == PackageMark.MarkedForUpdate);

                    var toreinstall = PackageItems.Count(n =>
                        n.Mark == PackageMark.MarkedForReinstallation);

                    var toremove = PackageItems.Count(n =>
                        n.Mark == PackageMark.MarkedForRemoval);

                    return string.Format("{0} packages listed, {1} installed, {2} to install, {3} to update, {4} to remove",
                        total, installed, toinstall, toupdate, toremove);
                }

                return string.Format("Loading package information ({0} %)",
                    _totalCount != 0 ? Math.Round(_currentCount * 1.0 / (_totalCount * 1.0) * 100) : 0);
            }
        }
    }
}
