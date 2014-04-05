using Mg2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Mg2.Views
{
    public partial class MainView
    {
        private readonly object _lockObject = new object();

        public MainView()
        {
            InitializeComponent();

            var iter = 0;
            var running = false;

            var filter = new Action(() =>
            {
                iter++;

                if (running)
                    return;

                Task.Run(() =>
                {
                    running = true;

                    while (iter > 0)
                    {
                        iter--;
                        Task.Delay(20).Wait();

                        if (iter == 0)
                        {
                            Task.Delay(80).Wait();
                        }
                    }

                    Filter();
                    running = false;
                });
            });

            FiltersBox.SelectionChanged += (sender, args) => filter();
            QueryBox.TextChanged += (sender, args) => filter();

            filter();
        }

        private void Filter()
        {
            IEnumerable<PackageItem> packageItems = null;
            string text = null;
            
            Dispatcher.Invoke(() =>
            {
                packageItems = (IEnumerable<PackageItem>)PackageItemsView.Tag;
                text = QueryBox.Text;
            });

            if (packageItems == null)
                return;

            IEnumerable<PackageItem> result;

            if (string.IsNullOrEmpty(text))
            {
                result = packageItems;
            }
            else
            {
                var operand = QueryParser.Parse(text);

                if (operand == null)
                    return;

                result = from packageItem in packageItems
                         where QueryMatcher.Match(packageItem, operand)
                         select packageItem;
            }

            PackageItemsView.Dispatcher.Invoke(() =>
            {
                lock (_lockObject)
                {
                    // Hiding the view improves performance a lot as it doesn't attempt to paint after each addition.
                    // Setting the ItemsSource instead of adding items to Items reduces performance.
                    
                    PackageItemsView.Visibility = Visibility.Hidden;

                    PackageItemsView.Items.Clear();

                    foreach (var item in result)
                        PackageItemsView.Items.Add(item);

                    if (PackageItemsView.SelectedIndex == -1)
                        PackageItemsView.SelectedIndex = 0;

                    PackageItemsView.Visibility = Visibility.Visible;
                }
            });
        }

        public void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender is Button)
                e.Handled = true;
        }
    }
}
