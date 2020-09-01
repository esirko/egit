using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using SharpDX.Direct2D1;
using System.Linq;
using DynamicData;
using Avalonia.Controls;
using ReactiveUI;
using System.Reactive;
using egit.Engine;

namespace egit.ViewModels
{
    public class ViewModel_RepoInfo : ViewModelBase
    {
        public ViewModel_RepoInfo()
        {
            Repos = Settings.Default.LocalRepos?.Cast<string>().ToList();
            _SelectedRepoOrOption = Settings.Default.LastSelectedLocalRepo;
            Initialized = true;
            GitEngine.Get().InitializeViewModel(this);
            GitEngine.Get().StartTraversingRepo(_SelectedRepoOrOption);
        }

        private List<string> Repos;
        private bool Initialized = false;

        public GitEngine GitRepoEngine { get { return GitEngine.Get();  } }

        public List<string> ReposWithOptions
        {
            get
            {
                List<string> temp = Repos.ToList();
                temp.Add("Other repo on disk...");
                return temp;
            }
        }

        private string _SelectedRepoOrOption;
        public string SelectedRepoOrOption
        {
            get
            {
                return _SelectedRepoOrOption;
            }
            set
            {
                _SelectedRepoOrOption = value;

                if (_SelectedRepoOrOption.EndsWith("..."))
                {
                    var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("TODO", "TODO: need ability to browse to a new repo");
                    messageBoxStandardWindow.Show();
                }
                else
                {
                    if (Initialized)
                    {
                        Settings.Default.LastSelectedLocalRepo = _SelectedRepoOrOption;
                        Settings.Default.Save();
                        GitEngine.Get().StartTraversingRepo(_SelectedRepoOrOption);
                    }
                }
            }
        }
    }
}
