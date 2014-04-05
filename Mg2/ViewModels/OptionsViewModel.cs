using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Mg2.Models;

namespace Mg2.ViewModels
{
    public class OptionsViewModel : Screen
    {
        public Options Options { get; set; }

        public ICommand CloseCommand { get; set; }

        public OptionsViewModel(Options options)
        {
            DisplayName = "Options";
            Options = options;
            CloseCommand = new RelayCommand(TryClose);
        }
    }
}