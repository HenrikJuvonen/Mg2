using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MgKit.Model.Interface;

namespace MgKit.Model
{
    public class PackageAction : IPackageAction, INotifyPropertyChanged
    {
        private string _status;
        private int _progress;
        private bool _isIndeterminate;

        public List<PackageAction> Actions { get; set; } 

        public IPackage Package { get; private set; }
        public string Type { get; private set; }

        public string Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged(); }
        }

        public int Progress
        {
            get { return _progress; }
            set
            { 
                _progress = value; OnPropertyChanged();
                if (Progress == 100) IsIndeterminate = false;
            }
        }

        public bool IsIndeterminate
        {
            get { return _isIndeterminate; }
            set { _isIndeterminate = value; OnPropertyChanged(); }
        }

        public PackageAction(IPackage package, string type)
        {
            Actions = new List<PackageAction>();
            Package = package;
            Type = type;
        }

        public void AddAction(PackageAction action)
        {
            Actions.Add(action);
            foreach (PropertyChangedEventHandler eventHandler in PropertyChanged.GetInvocationList())
            {
                action.SubscribeToPropertyChanged(eventHandler);
            }
            foreach (Action<PackageAction> eventHandler in ActionAdded.GetInvocationList())
            {
                action.SubscribeToActionAdded(eventHandler);
            }
            ActionAdded(action);
        }

        public void SubscribeToActionAdded(Action<PackageAction> eventHandler)
        {
            ActionAdded += eventHandler;
            foreach (var action in Actions)
            {
                action.SubscribeToActionAdded(eventHandler);
            }
        }

        public event Action<PackageAction> ActionAdded = delegate { };

        public void SubscribeToPropertyChanged(PropertyChangedEventHandler eventHandler)
        {
            PropertyChanged += eventHandler;
            foreach (var action in Actions)
            {
                action.SubscribeToPropertyChanged(eventHandler);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public static IEnumerable<PackageAction> OrderActionsByType(IEnumerable<PackageAction> actions)
        {
            return actions.OrderBy(action =>
            {
                switch (action.Type)
                {
                    case "fetch":
                        return 0;
                    case "install":
                        return 1;
                    case "reinstall":
                        return 2;
                    case "uninstall":
                        return 3;
                    case "update":
                        return 4;
                    case "lock":
                        return 5;
                    case "unlock":
                        return 6;
                    default:
                        return 7;
                }
            }).ToArray();
        }
    }
}