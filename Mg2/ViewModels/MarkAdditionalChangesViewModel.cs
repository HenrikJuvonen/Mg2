using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Mg2.Models;

namespace Mg2.ViewModels
{
    public class MarkAdditionalChangesViewModel : Screen
    {
        public List<TreeViewItem> List { get; private set; }

        public MarkAdditionalChangesViewModel(IEnumerable<PackageItem> installed, IEnumerable<PackageItem> updated, IEnumerable<PackageItem> removed)
        {
            DisplayName = "";

            List = new List<TreeViewItem>();

            if (installed != null && installed.Any())
                List.Add(new TreeViewItem
                    {
                        Header = "To be installed",
                        ItemsSource = installed.OrderBy(n => n.Name).ThenBy(n => n.Version),
                        IsExpanded = true
                    });

            if (updated != null && updated.Any())
                List.Add(new TreeViewItem
                    {
                        Header = "To be updated",
                        ItemsSource = updated.OrderBy(n => n.Name).ThenBy(n => n.Version),
                        IsExpanded = true
                    });

            if (removed != null && removed.Any())
                List.Add(new TreeViewItem
                    {
                        Header = "To be removed",
                        ItemsSource = removed.OrderBy(n => n.Name).ThenBy(n => n.Version),
                        IsExpanded = true
                    });

            CancelCommand = new RelayCommand(() => TryClose(false));
            MarkCommand = new RelayCommand(() => TryClose(true));
        }

        public ICommand CancelCommand { get; set; }
        public ICommand MarkCommand { get; set; }
    }
}