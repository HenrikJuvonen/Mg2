using MgKit.Model;
using MgKit.Model.Interface;
using MgKit.Model.Mock;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MgKit
{
    static class Program
    {
        static void Main()
        {
            var manager = new MockPackageManager();

            var actions = new List<PackageAction>();

            var test10 = manager.Packages.Single(n => n.Id == "test" && n.Version == "1.0");
            var test20 = manager.Packages.Single(n => n.Id == "test" && n.Version == "2.0");
            var other11 = manager.Packages.Single(n => n.Id == "other" && n.Version == "1.1");
            var other12 = manager.Packages.Single(n => n.Id == "other" && n.Version == "1.2");
            var other13 = manager.Packages.Single(n => n.Id == "other" && n.Version == "1.3");

            var packages = new List<IPackage>
            {
                test10,
                other11
            };

            foreach (var package in packages)
            {
                actions.Add(new PackageAction(package, "unlock"));
                actions.Add(new PackageAction(package, "lock"));
                actions.Add(new PackageAction(package, "uninstall"));
                actions.Add(new PackageAction(package, "update"));
                actions.Add(new PackageAction(package, "reinstall"));
                actions.Add(new PackageAction(package, "install"));
                actions.Add(new PackageAction(package, "fetch"));
            }

            var task = manager.Execute(actions.ToArray());

            foreach (var action in actions)
            {
                action.SubscribeToPropertyChanged((sender, args) =>
                {
                    var a = (PackageAction) sender;
                    Console.WriteLine("{0,-15} | {1,-15} | {2,-9} | {3,-15} | {4,-3}", DateTime.Now, a.Package, a.Type, a.Status, a.Progress);
                });
            }

            task.Wait();

            Console.WriteLine();
            Console.WriteLine("Installed packages:");

            var installed = manager.Packages.Where(n => n.Flags.Any(m => m == "installed"));

            foreach (var package in installed)
            {
                Console.WriteLine("{0,-15}", package);
            }

            Console.Read();
        }
    }
}