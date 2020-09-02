using System;
using System.Collections.Generic;
using System.Text;
using egit.Engine;
using egit.Models;

namespace egit.ViewModels
{
    public class ViewModel_CommitFileList : ViewModelBase
    {
        public GitEngine GitRepoEngine { get { return GitEngine.Get(); } }
    }
}
