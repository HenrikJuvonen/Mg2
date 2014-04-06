using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Mg2.ViewModels;
using System.IO;
using System.Runtime;
using System.Windows;
using MgKit.Interface;
using MgKit.OneGet;

namespace Mg2
{
    public partial class App
    {
        public App()
        {
            Directory.CreateDirectory(Constants.AppDataPath);
            ProfileOptimization.SetProfileRoot(Constants.AppDataPath);
            ProfileOptimization.StartProfile("Startup.Profile");

            DispatcherHelper.Initialize();

            var ioc = SimpleIoc.Default;
            ioc.Register<IWindowManager>(() => new WindowManager());
            ioc.Register<IPackageManager>(() => new OneGetPackageManager());

            var windowManager = ioc.GetInstance<IWindowManager>();
            
            DispatcherUnhandledException += (sender, args) =>
            {
                windowManager.ShowDialog(new ExceptionViewModel(args.Exception));
                args.Handled = true;
            };
        }

        public void OnStartup(object sender, StartupEventArgs e)
        {
            var windowManager = SimpleIoc.Default.GetInstance<IWindowManager>();
            windowManager.ShowWindow(new MainViewModel());
        }
    }
}