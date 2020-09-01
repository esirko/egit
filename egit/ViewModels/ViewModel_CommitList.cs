using System;
using System.Collections.Generic;
using System.Text;
using egit.Engine;
using egit.Models;

namespace egit.ViewModels
{
    public class ViewModel_CommitList : ViewModelBase
    {
        public CommitViewEnumerable Commits { get
            {
                return GitEngine.Get().CurrentViewOfCommits;
            } }

        /*
        public List<string> Commits
        {
            get
            {
                List<string> temp = new List<string>() { "1", "2", "3" };
                temp.Add("Other repo on disk...");
                return temp;
            }
        }
        */


    }
}
