using System.Windows.Input;
using GalaSoft.MvvmLight.Command;

namespace Mg2.ViewModels
{
    public class AboutViewModel : Screen
    {
        public AboutViewModel()
        {
            DisplayName = "About Mg2";
            CloseCommand = new RelayCommand(TryClose);
        }

        public ICommand CloseCommand { get; set; }
    }
}