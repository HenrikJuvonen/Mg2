using System;
using GalaSoft.MvvmLight.Command;
using Mg2.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Mg2.ViewModels
{
    public class FiltersViewModel : Screen
    {
        private Filter _selectedFilter;

        public ObservableCollection<Filter> Filters { get; private set; }

        public ICommand AddCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        public Filter SelectedFilter
        {
            get { return _selectedFilter; }
            set
            {
                _selectedFilter = value;
                RaisePropertyChanged(() => SelectedFilter);
            }
        }

        public FiltersViewModel(ObservableCollection<Filter> filters)
        {
            DisplayName = "Filters";
            Filters = filters;

            AddCommand = new RelayCommand(Add);
            RemoveCommand = new RelayCommand(Remove);
            CloseCommand = new RelayCommand(Close);
        }

        public void Add()
        {
            Filters.Add(new Filter("New filter", ""));
        }

        public void Remove()
        {
            Filters.Remove(SelectedFilter);
        }

        private void Close()
        {
            try
            {
                var path = Constants.AppDataPath + "Filters.xml";

                var writer = new XmlSerializer(typeof (ObservableCollection<Filter>), new XmlRootAttribute("Filters"));
                var file = new StreamWriter(path);
                writer.Serialize(file, Filters);
                file.Close();
            }
            catch
            {
                Console.WriteLine("Could not save filters");
            }

            TryClose();
        }
    }
}
