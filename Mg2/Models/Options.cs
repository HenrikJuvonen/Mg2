using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using MgKit.Model;
using MgKit.Model.Interface;

namespace Mg2.Models
{
    public class Options : ObservableObject
    {
        private readonly IPackageSourceProvider _packageSourceProvider;

        public Options()
        {
            _packageSourceProvider = SimpleIoc.Default.GetInstance<IPackageManager>().PackageSourceProvider;
        }

        public IEnumerable<IPackageSource> PackageSources
        {
            get { return _packageSourceProvider.LoadPackageSources(); }
        }

        public void AddPackageSource()
        {
            _packageSourceProvider.SavePackageSources(PackageSources.Concat(new[] { new PackageSource("http://chocolatey.org/api/v2", "new", "Chocolatey") }));
        }

        public void RemovePackageSource(IPackageSource packageSource)
        {
            _packageSourceProvider.SavePackageSources(PackageSources.Except(new[] { packageSource }));
        }

        public void UpdatePackageSources(IEnumerable<IPackageSource> packageSources)
        {
            _packageSourceProvider.SavePackageSources(packageSources);
        }

        public void Load()
        {
            try
            {
                var path = Constants.AppDataPath + "Options.xml";

                var reader = new XmlSerializer(typeof(Options), new XmlRootAttribute("Options"));
                var file = new StreamReader(path);
                var options = (Options)reader.Deserialize(file);
                file.Close();

                AskToConfirmChangesThatAlsoAffectOtherPackages = options.AskToConfirmChangesThatAlsoAffectOtherPackages;
                ClickingOnTheStatusIconMarksTheMostLikelyAction = options.ClickingOnTheStatusIconMarksTheMostLikelyAction;
            }
            catch
            {
                AskToConfirmChangesThatAlsoAffectOtherPackages = true;
            }
        }

        public void Save()
        {
            try
            {
                var path = Constants.AppDataPath;
                Directory.CreateDirectory(path);

                var writer = new XmlSerializer(typeof(Options), new XmlRootAttribute("Options"));
                var file = new StreamWriter(path + "Options.xml");
                writer.Serialize(file, this);
                file.Close();
            }
            catch
            {
            }
        }
        
        private bool _askToConfirmChangesThatAlsoAffectOtherPackages;
        public bool AskToConfirmChangesThatAlsoAffectOtherPackages
        {
            get
            {
                return _askToConfirmChangesThatAlsoAffectOtherPackages;
            }
            set
            {
                _askToConfirmChangesThatAlsoAffectOtherPackages = value;
                RaisePropertyChanged(() => AskToConfirmChangesThatAlsoAffectOtherPackages);
                Save();
            }
        }

        private bool _clickingOnTheStatusIconMarksTheMostLikelyAction;
        public bool ClickingOnTheStatusIconMarksTheMostLikelyAction
        {
            get
            {
                return _clickingOnTheStatusIconMarksTheMostLikelyAction;
            }
            set
            {
                _clickingOnTheStatusIconMarksTheMostLikelyAction = value;
                RaisePropertyChanged(() => ClickingOnTheStatusIconMarksTheMostLikelyAction);
                Save();
            }
        }
    }
}
