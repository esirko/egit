using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace egit.ViewModels
{
    public class ViewModel_RepoInfo : ViewModelBase
    {
        public ObservableCollection<string> Repos { get { return new ObservableCollection<string>(new List<string>() { "abc", "def" }); } }

        private List<String> _property;
        public List<String> Property
        {
            get
            {
                return new List<string>() { "string1", "string2" };
            }
            set {
                _property = value;
            }
        }

        public string SimpleStringProperty { get; set; }

        public IEnumerable<string> Repos2 { get { return new List<string>() { "abc", "def" }; } }
    }
}
