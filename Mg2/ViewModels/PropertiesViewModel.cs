using System;
using GalaSoft.MvvmLight.Command;
using Mg2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mg2.ViewModels
{
    public class PropertiesViewModel : Screen
    {
        public PackageItem PackageItem { get; private set; }
        public IEnumerable<PackageItem> Dependencies { get; private set; }
        public IEnumerable<PackageItem> Dependants { get; private set; }
        public IEnumerable<TreeViewItem> Files { get; private set; }
        
        public ICommand CloseCommand { get; set; }

        public PropertiesViewModel(PackageItem packageItem)
        {
            DisplayName = "Properties";
            PackageItem = packageItem;
            CloseCommand = new RelayCommand(TryClose);
            GetFiles();
        }

        private void GetFiles()
        {
            try
            {
                Files = PackageItem.PackageIdentity.Files.Select(file => new TreeViewItem { Header = file.Path }).ToArray();
                RaisePropertyChanged(() => Files);
            }
            catch
            {
                Console.WriteLine("Could not get files");
            }
        }
    }
}