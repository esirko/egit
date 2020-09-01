using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using LibGit2Sharp;

namespace egit.Models
{
    public class HackyFileOrFolder : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        SortedList<string, HackyFileOrFolder> _Entries;
        public SortedList<string, HackyFileOrFolder> Entries { get { return _Entries; }  set
            {
                _Entries = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<HackyFileOrFolder> SortedObservableEntries { get // TODO: does this really need to be ObservableCollection if the class is INotifyPropertyChangd?
            {
                return new ObservableCollection<HackyFileOrFolder>(Entries.Select(x => x.Value));
            }
        }

        string _Name;
        public string Name { get { return _Name; } set
            {
                _Name = value;
                OnPropertyChanged();
            }
        }
        public List<Commit> History = new List<Commit>();
        public HackyFileOrFolder Parent;

        public HackyFileOrFolder(string n, HackyFileOrFolder p)
        {
            Entries = new SortedList<string, HackyFileOrFolder>();
            Name = n;
            Parent = p;
        }

        public void Clear()
        {
            Entries.Clear();
            History.Clear();
            Parent = null;
        }

        public string GetFullPath()
        {
            return (Parent != null) ? Parent.GetFullPath() + "/" + Name : Name;
        }
    }

    public class HackyFile : HackyFileOrFolder
    {
        public HackyFile(string n, HackyFileOrFolder p)
            : base(n, p)
        {

        }
    }

    public class HackyFolder : HackyFileOrFolder
    {
        public HackyFolder(string n, HackyFileOrFolder p)
            : base(n, p)
        {

        }
    }


    public class FileSystem
    {
        public HackyFileOrFolder BaseFolder = new HackyFileOrFolder("", null);

        internal void PopulateWithFileAndCommitPair(string path, Commit c1)
        {
            HackyFileOrFolder currentFolder = BaseFolder;

            string[] directories = path.Split('/', '\\');
            for (int i = 0; i < directories.Length; i++)
            {
                bool isFile = i == directories.Length - 1;
                HackyFileOrFolder next;
                if (!currentFolder.Entries.ContainsKey(directories[i]))
                {
                    HackyFileOrFolder fileOrFolder;
                    if (isFile)
                    {
                        fileOrFolder = new HackyFile(directories[i], currentFolder);
                    }
                    else
                    {
                        fileOrFolder = new HackyFolder(directories[i], currentFolder);
                    }
                    currentFolder.Entries[directories[i]] = fileOrFolder;
                }
                next = currentFolder.Entries[directories[i]];
                if (next.History.Count == 0 || next.History.Last() != c1)
                {
                    next.History.Add(c1);
                }
                currentFolder = next;
            }
        }

        internal void Clear()
        {
            BaseFolder.Clear();
        }

    }
}
