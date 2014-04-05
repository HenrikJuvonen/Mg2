using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Mg2.Models;

namespace Mg2.ViewModels
{
    public class SummaryViewModel : Screen
    {
        public ICommand CancelCommand { get; set; }
        public ICommand ApplyCommand { get; set; }

        public List<TreeViewItem> List { get; private set; }
        
        public bool ButtonsEnabled { get; private set; }

        public string InstalledCount
        {
            get
            {
                var count = List.Where(n => (string)n.Header == "To be installed").SelectMany(n => n.Items.OfType<PackageItem>()).Count();

                if (count == 0)
                    return null;

                return string.Format("{0} package{1} will be installed", count, count > 1 ? "s" : null);
            }
        }

        public string UpdatedCount
        {
            get
            {
                var count = List.Where(n => (string)n.Header == "To be updated").SelectMany(n => n.Items.OfType<PackageItem>()).Count();

                if (count == 0)
                    return null;

                return string.Format("{0} package{1} will be updated", count, count > 1 ? "s" : null);
            }
        }

        public string ReinstalledCount
        {
            get
            {
                var count = List.Where(n => (string)n.Header == "To be reinstalled").SelectMany(n => n.Items.OfType<PackageItem>()).Count();

                if (count == 0)
                    return null;

                return string.Format("{0} package{1} will be reinstalled", count, count > 1 ? "s" : null);
            }
        }

        public string RemovedCount
        {
            get
            {
                var count = List.Where(n => (string)n.Header == "To be removed").SelectMany(n => n.Items.OfType<PackageItem>()).Count();

                if (count == 0)
                    return null;

                return string.Format("{0} package{1} will be removed", count, count > 1 ? "s" : null);
            }
        }

        public SummaryViewModel(IEnumerable<PackageItem> installed, IEnumerable<PackageItem> updated, IEnumerable<PackageItem> reinstalled, IEnumerable<PackageItem> removed)
        {
            DisplayName = "";

            ButtonsEnabled = true;

            List = new List<TreeViewItem>();

            if (installed != null && installed.Any())
                List.Add(new TreeViewItem { Header = "To be installed", ItemsSource = installed, IsExpanded = true });

            if (updated != null && updated.Any())
                List.Add(new TreeViewItem { Header = "To be updated", ItemsSource = updated, IsExpanded = true });

            if (reinstalled != null && reinstalled.Any())
                List.Add(new TreeViewItem { Header = "To be reinstalled", ItemsSource = reinstalled, IsExpanded = true });

            if (removed != null && removed.Any())
                List.Add(new TreeViewItem { Header = "To be removed", ItemsSource = removed, IsExpanded = true });

            CancelCommand = new RelayCommand(() => TryClose(false));
            ApplyCommand = new RelayCommand(() => TryClose(true));
        }
    }
}