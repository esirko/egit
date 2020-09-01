using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using egit.Engine;
using egit.Models;

namespace egit.ViewModels
{
    public class ViewModel_FileTree : ViewModelBase
    {
        public GitEngine GitRepoEngine { get { return GitEngine.Get(); } }

        public ObservableCollection<HackyFileOrFolder> RootDirectoriesAndFiles { get
            {
                return new ObservableCollection<HackyFileOrFolder>(GitRepoEngine.HistoryFS.BaseFolder.Entries.Select(x => x.Value));
            } }
    }
}
