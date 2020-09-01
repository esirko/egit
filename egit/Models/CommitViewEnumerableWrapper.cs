using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using egit.Views;

namespace egit.Models
{
    public class CommitViewEnumerableWrapper : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private CommitViewEnumerable _CurrentViewOfCommits = new CommitViewEnumerable(new List<Changelist>());
        public CommitViewEnumerable Commits
        {
            get { return _CurrentViewOfCommits; }
            set
            {
                _CurrentViewOfCommits = value;
                OnPropertyChanged();
            }
        }

        CommitWrapper _SelectedCommit;
        private Action<CommitWrapper, CommitWrapper> HandleMainSelectedCommitChanged;

        public CommitViewEnumerableWrapper(Action<CommitWrapper, CommitWrapper> handleMainSelectedCommitChanged)
        {
            HandleMainSelectedCommitChanged = handleMainSelectedCommitChanged;
        }

        View_CommitList CommitListView;
        internal void RegisterCommitList(View_CommitList commitListView)
        {
            // TODO: I have to make this ViewModelly-class aware of purely View classes, which is not ideal. 
            // I have to do it because DataGrid doesn't support binding to SelectedItems.
            CommitListView = commitListView;
        }

        public CommitWrapper SelectedCommit
        {
            get
            {
                return _SelectedCommit;
            }
            set
            {
                _SelectedCommit = value;
                OnPropertyChanged();

                var x = CommitListView.DataGridCommitList.SelectedItems;
                HandleMainSelectedCommitChanged(_SelectedCommit, null);
            }
        }
    }

}
