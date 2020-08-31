using System;
using System.Collections.Generic;
using System.Text;

namespace egit.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia!";

        public MainWindowViewModel()
        {
            RepoInfo = new ViewModel_RepoInfo();
        }

        public ViewModel_RepoInfo RepoInfo { get; }
    }
}
