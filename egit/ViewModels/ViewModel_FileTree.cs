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
                return GitRepoEngine.HistoryFS.BaseFolder.Entries;
            } }

        HackyFileOrFolder _SelectedFileOrFolder;
        public HackyFileOrFolder SelectedFileOrFolder
        {
            get { return _SelectedFileOrFolder; }
            set
            {
                _SelectedFileOrFolder = value;
                GitRepoEngine.SelectedScopeChanged(_SelectedFileOrFolder);
            }
        }
    }
}
