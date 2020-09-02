using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Avalonia.Threading;
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

        // I guess we need to keep these collections in sync.. one is used for binding as an ObservableCollection, the other for sorting in a dictionary for fast lookup
        Dictionary<string, HackyFileOrFolder> EntriesDictionary;
        ObservableCollection<HackyFileOrFolder> _Entries;
        public ObservableCollection<HackyFileOrFolder> Entries
        {
            get { return _Entries; }// return new ObservableCollection<HackyFileOrFolder>(EntriesDictionary.Values); }
            set
            {
                _Entries = value;
                OnPropertyChanged();
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
            EntriesDictionary = new Dictionary<string, HackyFileOrFolder>(StringComparer.Ordinal);
            Entries = new ObservableCollection<HackyFileOrFolder>();
            Name = n;
            Parent = p;
        }

        public void Clear()
        {
            EntriesDictionary.Clear();
            Entries.Clear();
            History.Clear();
            Parent = null;
        }

        public string GetFullPath()
        {
            return (Parent != null) ? Parent.GetFullPath() + "/" + Name : Name;
        }

        internal bool TryGetValue(string key, out HackyFileOrFolder fileOrFolder)
        {
            return EntriesDictionary.TryGetValue(key, out fileOrFolder);
        }

        internal void Add(string key, HackyFileOrFolder fileOrFolder)
        {
            EntriesDictionary.Add(key, fileOrFolder);
            OnPropertyChanged("Entries");
            Dispatcher.UIThread.Post(() =>
            {
                int i = -1;
                int k = Entries.Count;
                while (k - i > 1)
                {
                    int j = (k - i) / 2 + i;
                    int comparison = StringComparer.Ordinal.Compare(Entries[j].Name, fileOrFolder.Name);

                    if (comparison > 0)
                    {
                        k = j;
                    }
                    else if (comparison < 0)
                    {
                        i = j;
                    }
                    else if (comparison == 0)
                    {
                        throw new Exception("Unexpected string match: this should have been culled out before getting here");
                    }
                }

                Entries.Insert(k, fileOrFolder);
            });
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
                if (!currentFolder.TryGetValue(directories[i], out HackyFileOrFolder fileOrFolder))
                {
                    bool isFile = i == directories.Length - 1;
                    if (isFile)
                    {
                        fileOrFolder = new HackyFile(directories[i], currentFolder);
                    }
                    else
                    {
                        fileOrFolder = new HackyFolder(directories[i], currentFolder);
                    }

                    currentFolder.Add(directories[i], fileOrFolder);
                }
                if (fileOrFolder.History.Count == 0 || fileOrFolder.History.Last() != c1)
                {
                    fileOrFolder.History.Add(c1);
                }
                currentFolder = fileOrFolder;


                /*
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
                */
            }
        }

        internal void Clear()
        {
            BaseFolder.Clear();
        }

    }
}
