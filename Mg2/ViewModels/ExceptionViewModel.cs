using System;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Mg2.Extensions;

namespace Mg2.ViewModels
{
    public class ExceptionViewModel : Screen
    {
        private readonly AggregateException _exception;

        public ICommand CloseCommand { get; set; }
        
        public string Exceptions
        {
            get
            {
                var exceptions = "";

                foreach (var exception in _exception.InnerExceptions.Reverse())
                {
                    exceptions += exception.Message + "\r\n";
                    exceptions += exception.StackTrace + "\r\n\r\n";
                }

                return exceptions;
            }
        }

        public ExceptionViewModel(Exception exception)
        {
            DisplayName = "An unhandled exception occurred";
            CloseCommand = new RelayCommand(TryClose);

            _exception = exception.Unwrap().ToAggregate();
        }
    }
}