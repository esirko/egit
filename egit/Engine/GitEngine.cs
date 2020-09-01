using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using egit.ViewModels;
using LibGit2Sharp;
using MessageBox.Avalonia;

namespace egit.Engine
{
    public class GitEngine : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public static GitEngine Get()
        {
            if (_Singleton == null)
            {
                _Singleton = new GitEngine();
            }
            return _Singleton;
        }

        internal void InitializeViewModel(ViewModel_RepoInfo repoInfo)
        {
            RepoInfo = repoInfo;
        }





        public int Counter
        {
            get { return _Counter; }
            set
            {
                _Counter = value;
                OnPropertyChanged();
            }
        }

        private List<string> _Branches;
        public List<string> Branches { get { return _Branches; } private set { _Branches = value; OnPropertyChanged(); }  }

        private string _SelectedBranch;
        public string SelectedBranch { get { return _SelectedBranch; } set { _SelectedBranch = value; OnPropertyChanged(); } }



        public void StartTraversingRepo(string repoPath)
        {
            if (repoPath != CurrentRepoPath)
            {
                // TODO: Abort all other current work

                if (Repo != null)
                {
                    Repo.Dispose();
                    Repo = null;
                    CurrentSelectedBranch = null;
                }

                CurrentRepoPath = repoPath;

                if (!string.IsNullOrEmpty(CurrentRepoPath))
                {
                    if (Repository.IsValid(CurrentRepoPath))
                    {
                        Repo = new Repository(CurrentRepoPath);
                        CurrentSelectedBranch = Repo.Head;
                    }
                    else
                    {
                        MessageBoxManager.GetMessageBoxStandardWindow("Bad repo", $"Not a valid repo: {CurrentRepoPath}");
                    }
                }


                delayTime /= 2;
                Task t = new Task(async () => { await DoStuffAsync(); });
                Task t2 = new Task(async () => { await RefreshComboBoxBranchesAsync(); });
                t.Start();
                t2.Start();
            }
        }





        private async Task DoStuffAsync()
        {
            for (int i = 0; i < 60; i++)
            {
                await Task.Delay(delayTime);
                Counter++;
            }
        }

        private async Task RefreshComboBoxBranchesAsync()
        {
            string enumeratingBranches = "Enumerating branches...";
            Branches = new List<string>() { enumeratingBranches };
            SelectedBranch = enumeratingBranches;

            List<string> branches = new List<string>();
            foreach (Branch b in Repo.Branches)
            {
                if (!b.FriendlyName.StartsWith("full")) // TODO: what is this? Probably a remnant of something that can be deleted now
                {
                    if (b.IsCurrentRepositoryHead)
                    {
                        IndexOfHeadBranch = branches.Count;
                    }
                    branches.Add(b.FriendlyName);
                }
                await Task.Yield();
            }
            Branches = branches;
            SelectedBranch = Branches[IndexOfHeadBranch];
        }

        internal Branch CurrentSelectedBranch = null;
        internal int IndexOfHeadBranch = -1; // TODO: refactor this, shouldn't store as int

        internal Repository Repo;

        private static GitEngine _Singleton;
        private string CurrentRepoPath;
        int delayTime = 2000;
        private int _Counter;
        private ViewModel_RepoInfo RepoInfo;

    }
}
