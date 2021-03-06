﻿using System;
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
                return GitRepoEngine.UserFS.BaseFolder.Entries;
            }
        }

        HackyFileOrFolder _SelectedUser;
        public HackyFileOrFolder SelectedUser
        {
            get { return _SelectedUser; }
            set
            {
                _SelectedUser = value;
                GitRepoEngine.SelectedScopeChanged(_SelectedUser);
            }
        }

    }
}
