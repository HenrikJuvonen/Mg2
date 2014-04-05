using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Mg2.Models;

namespace Mg2.Converters
{

    public class PackageMarkToButtonBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mark = (PackageMark)value;

            switch (mark)
            {
                case PackageMark.NotInstalled:
                case PackageMark.NotInstalledNew:
                case PackageMark.MarkedForInstallation:
                    return Brushes.White;
                case PackageMark.Installed:
                case PackageMark.MarkedForReinstallation:
                case PackageMark.MarkedForUpdate:
                    return Brushes.Green;
                case PackageMark.InstalledUpdatable:
                    return Brushes.Silver;
                case PackageMark.Broken:
                case PackageMark.MarkedForRemoval:
                    return Brushes.Red;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class PackageMarkToButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mark = (PackageMark)value;

            switch (mark)
            {
                case PackageMark.NotInstalledNew:
                    return "N";
                case PackageMark.MarkedForInstallation:
                case PackageMark.MarkedForReinstallation:
                    return "I";
                case PackageMark.MarkedForUpdate:
                    return "U";
                case PackageMark.MarkedForRemoval:
                    return "R";
                case PackageMark.Broken:
                case PackageMark.InstalledUpdatable:
                    return "!";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class PackageMarkToCellBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mark = (PackageMark)value;

            switch (mark)
            {
                case PackageMark.MarkedForInstallation:
                    return Brushes.LimeGreen;
                case PackageMark.MarkedForReinstallation:
                    return Brushes.Green;
                case PackageMark.MarkedForUpdate:
                    return Brushes.Yellow;
                case PackageMark.MarkedForRemoval:
                    return Brushes.Red;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
