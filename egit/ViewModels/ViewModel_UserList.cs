using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using egit.Engine;
using egit.Models;

namespace egit.ViewModels
{
    public class ViewModel_UserList : ViewModelBase
    {
        public GitEngine GitRepoEngine { get { return GitEngine.Get(); } }

        public ObservableCollection<HackyFileOrFolder> Users
        {
            get
            {
                return new ObservableCollection<HackyFileOrFolder>(GitRepoEngine.UserFS.BaseFolder.Entries.Select(x => x.Value));
            }
        }

    }
}
