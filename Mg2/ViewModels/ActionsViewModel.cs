using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using MgKit;
using MgKit.Interface;

namespace Mg2.ViewModels
{
    public class ActionsViewModel : Screen
    {
        public bool CanClose { get; private set; }

        public ObservableCollection<PackageAction> List { get; private set; }

        public IEnumerable<PackageAction> OrderedList
        {
            get { return PackageAction.OrderActionsByType(List); }
        }

        public ICommand CloseCommand { get; private set; }

        public ActionsViewModel(IPackageManager packageManager, List<PackageAction> actions)
        {
            DisplayName = "";

            List = new ObservableCollection<PackageAction>(actions);
            List.CollectionChanged += (sender, args) => RaisePropertyChanged(() => OrderedList);

            var eventHandler = new PropertyChangedEventHandler((sender, args) =>
            {
                if (List.Any(n => n.Progress != 100))
                    return;

                CanClose = true;
                RaisePropertyChanged(() => CanClose);

                // TODO: if autoclose in options: TryClose(true)
            });

            actions.ForEach(n => n.SubscribeToPropertyChanged(eventHandler));
            
            foreach (var action in actions)
            {
                action.ActionAdded += a => DispatcherHelper.CheckBeginInvokeOnUI(() => List.Add(a));
            }

            CloseCommand = new RelayCommand(() => TryClose(true));
            
            packageManager.Execute(actions.ToArray());
        }
    }
}