using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Text;

namespace egit.ViewModels
{
    class ViewModel_FileTree : ViewModelBase
    {
        public ObservableCollection<string> FileTreeItems => new ObservableCollection<string>(new List<string>() { "abc", "def" });
    }
}
