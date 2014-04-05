using System.Xml.Serialization;
using GalaSoft.MvvmLight;

namespace Mg2.Models
{
    public class Filter : ObservableObject
    {
        private string _name;
        private string _query;

        [XmlAttribute("Name")]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        [XmlText]
        public string Query
        {
            get { return _query; }
            set
            {
                _query = value;
                RaisePropertyChanged(() => Query);
            }
        }

        public Filter()
        {
        }

        public Filter(string name, string query)
        {
            Name = name;
            Query = query;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
