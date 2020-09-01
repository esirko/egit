using System;
using System.Collections.Generic;
using System.Text;
using egit.Engine;

namespace egit.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia!";

        public MainWindowViewModel()
        {
            RepoInfo = new ViewModel_RepoInfo();
            MainCommitList = new ViewModel_CommitList();
            SecondaryCommitList = new ViewModel_CommitList();
        }

        public ViewModel_RepoInfo RepoInfo { get; }
        public ViewModel_CommitList MainCommitList { get; }
        public ViewModel_CommitList SecondaryCommitList { get; }

        public GitEngine GitRepoEngine { get { return GitEngine.Get(); } }
    }
}
