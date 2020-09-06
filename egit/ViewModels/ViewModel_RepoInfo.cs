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
using egit.Views;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace egit.ViewModels
{
    public class ViewModel_RepoInfo : ViewModelBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ViewModel_RepoInfo()
        {
            _SelectedRepoOrOption = Settings.Default.LastSelectedLocalRepo;
            Initialized = true;
            GitEngine.Get().StartTraversingNewRepo(_SelectedRepoOrOption);
        }

        private bool Initialized = false;
        private View_RepoInfo MyView;

        internal void RegisterView(View_RepoInfo view_RepoInfo)
        {
            MyView = view_RepoInfo;
        }

        public GitEngine GitRepoEngine { get { return GitEngine.Get();  } }

        public List<string> ReposWithOptions
        {
            get
            {
                List<string> temp = Settings.Default.LocalRepos?.Cast<string>().ToList();
                temp.Sort();
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
                if (value != null)
                {
                    if (value.EndsWith("..."))
                    {
                        Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            OpenFolderDialog ofd = new OpenFolderDialog();
                            ofd.Directory = _SelectedRepoOrOption;
                            string path = await ofd.ShowAsync((Window)MyView.GetVisualRoot());
                            if (!string.IsNullOrEmpty(path))
                            {
                                Settings.Default.LocalRepos.Add(path);
                                Settings.Default.Save();
                                OnPropertyChanged("ReposWithOptions");
                                SelectedRepoOrOption = path;
                            }
                        });
                    }
                    else
                    {
                        _SelectedRepoOrOption = value;
                        if (Initialized)
                        {
                            Settings.Default.LastSelectedLocalRepo = _SelectedRepoOrOption;
                            Settings.Default.Save();
                            GitEngine.Get().StartTraversingNewRepo(_SelectedRepoOrOption);
                        }
                        OnPropertyChanged();
                    }
                }
            }
        }
    }
}
