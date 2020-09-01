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
            MainCommitList = new ViewModel_CommitList(0);
            SecondaryCommitList = new ViewModel_CommitList(1);
            FileTree = new ViewModel_FileTree();
            UserList = new ViewModel_UserList();
            CommitInfo = new ViewModel_CommitInfo();
            CommitFileList = new ViewModel_CommitFileList();
        }

        public GitEngine GitRepoEngine { get { return GitEngine.Get(); } }

        public ViewModel_RepoInfo RepoInfo { get; }
        public ViewModel_CommitList MainCommitList { get; }
        public ViewModel_CommitList SecondaryCommitList { get; }
        public ViewModel_FileTree FileTree { get; }
        public ViewModel_UserList UserList { get; }
        public ViewModel_CommitInfo CommitInfo { get; }
        public ViewModel_CommitFileList CommitFileList { get; }
    }
}
