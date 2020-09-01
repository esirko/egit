using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using egit.Engine;
using egit.Models;

namespace egit.ViewModels
{
    public class ViewModel_CommitList : ViewModelBase
    {
        public ViewModel_CommitList(int isSecondary)
        {
            IsPrimary = isSecondary == 0;
            IsSecondary = isSecondary == 1;
        }

        public GitEngine GitRepoEngine { get { return GitEngine.Get(); } }

        public CommitViewEnumerableWrapper CommitList
        {
            get
            {
                if (IsPrimary)
                {
                    return GitRepoEngine.CurrentViewOfCommits;
                }
                else if (IsSecondary)
                {
                    return GitRepoEngine.CurrentlyDisplayedFeatureBranch;
                }
                else
                {
                    return null;
                }
            }
        }

        private readonly bool IsPrimary;
        private readonly bool IsSecondary;

    }
}
