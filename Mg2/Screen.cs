using System;
using System.Runtime.CompilerServices;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;

namespace Mg2
{
    // Based on Caliburn.Micro Screen-class

    public abstract class Screen : ViewModelBase
    {
        private string _displayName;
        private object _view;

        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; RaisePropertyChanged(); }
        }

        protected new void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.RaisePropertyChanged(propertyName);
        }

        protected Screen()
        {
            _displayName = GetType().FullName;
        }
        
        public void AttachView(object view)
        {
            _view = view;
        }

        Action GetViewCloseAction(bool? dialogResult)
        {
            var viewType = _view.GetType();

            var closeMethod = viewType.GetMethod("Close");

            if (closeMethod != null)
            {
                return () =>
                {
                    var isClosed = false;
                    if (dialogResult != null)
                    {
                        var resultProperty = _view.GetType().GetProperty("DialogResult");

                        if (resultProperty != null)
                        {
                            resultProperty.SetValue(_view, dialogResult, null);
                            isClosed = true;
                        }
                    }

                    if (!isClosed)
                    {
                        closeMethod.Invoke(_view, null);
                    }

                };
            }

            var isOpenProperty = viewType.GetProperty("IsOpen");

            if (isOpenProperty != null)
            {
                return () => isOpenProperty.SetValue(_view, false, null);
            }

            return () => { };
        }

        public void TryClose()
        {
            DispatcherHelper.RunAsync(() =>
            {
                var closeAction = GetViewCloseAction(null);
                closeAction();
            });
        }
        
        public void TryClose(bool? dialogResult)
        {
            DispatcherHelper.RunAsync(() =>
            {
                var closeAction = GetViewCloseAction(dialogResult);
                closeAction();
            });
        }
    }
}