using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using egit.Engine;
using egit.Models;
using egit.Views;
using LibGit2Sharp;

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

        internal bool RegisterView(View_CommitList view_CommitList)
        {
            if (IsPrimary)
            {
                GitRepoEngine.CurrentViewOfCommits.RegisterCommitList(view_CommitList);
            }
            else if (IsSecondary)
            {
                GitRepoEngine.CurrentlyDisplayedFeatureBranch.RegisterCommitList(view_CommitList);
            }
            return IsSecondary;
        }

        private readonly bool IsPrimary;
        private readonly bool IsSecondary;

    }
}
