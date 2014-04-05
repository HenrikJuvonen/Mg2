using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Mg2
{
    // Based on Caliburn.Micro WindowManager-class

    public interface IWindowManager
    {
        bool? ShowDialog(Screen screen);
        void ShowWindow(Screen screen);
    }

    public class WindowManager : IWindowManager
    {
        public bool? ShowDialog(Screen screen)
        {
            return CreateWindow(screen, true).ShowDialog();
        }

        public void ShowWindow(Screen screen)
        {
            CreateWindow(screen, false).Show();
        }

        private Window CreateWindow(Screen rootModel, bool isDialog)
        {
            var view = LocateView(rootModel);
            BindModel(rootModel, view);

            var window = EnsureWindow(view, isDialog);

            var binding = new Binding("DisplayName") { Mode = BindingMode.TwoWay };
            window.SetBinding(Window.TitleProperty, binding);

            return window;
        }

        private object LocateView(Screen model)
        {
            var viewModelName = model.GetType().Name;
            var viewName = viewModelName.Substring(0, viewModelName.LastIndexOf("Model", StringComparison.Ordinal));
            var type = GetType().Assembly.GetTypes().First(n => n.Name == viewName);
            return Activator.CreateInstance(type);
        }

        private static void BindModel(Screen model, object view)
        {
            var viewAsControl = (Control) view;
            viewAsControl.DataContext = model;
            model.AttachView(view);
        }

        private static Window EnsureWindow(object view, bool isDialog)
        {
            var window = view as Window;

            if (window == null)
            {
                window = new Window
                {
                    Content = view,
                    SizeToContent = SizeToContent.WidthAndHeight
                };

                var owner = InferOwnerOf(window);
                if (owner != null)
                {
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    window.Owner = owner;
                }
                else
                {
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }
            else
            {
                var owner = InferOwnerOf(window);
                if (owner != null && isDialog)
                {
                    window.Owner = owner;
                }
            }

            return window;
        }

        private static Window InferOwnerOf(Window window)
        {
            if (Application.Current == null)
                return null;

            var active = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            active = active ?? Application.Current.MainWindow;
            return Equals(active, window) ? null : active;
        }
    }
}
