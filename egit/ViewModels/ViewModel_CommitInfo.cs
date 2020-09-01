using System;
using System.Collections.Generic;
using System.Text;
using egit.Engine;

namespace egit.ViewModels
{
    public class ViewModel_CommitInfo : ViewModelBase
    {
        public GitEngine GitRepoEngine { get { return GitEngine.Get(); } }
    }
}
