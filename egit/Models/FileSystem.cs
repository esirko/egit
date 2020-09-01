using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibGit2Sharp;

namespace egit.Models
{
    public class HackyFileOrFolder
    {
        public SortedList<string, HackyFileOrFolder> Entries = new SortedList<string, HackyFileOrFolder>();
        public List<Commit> History = new List<Commit>();
        public HackyFileOrFolder Parent;
        public string Name;

        public HackyFileOrFolder(string n, HackyFileOrFolder p)
        {
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


    class FileSystem
    {
        public HackyFileOrFolder BaseFolder = new HackyFileOrFolder("", null);

        internal void PopulateWithFileAndCommitPair(string path, Commit c1)
        {
            HackyFileOrFolder currentFolder = BaseFolder;

            string[] directories = path.Split(Path.DirectorySeparatorChar);
            for (int i = 0; i < directories.Length; i++)
            {
                HackyFileOrFolder next;
                if (!currentFolder.Entries.ContainsKey(directories[i]))
                {
                    currentFolder.Entries[directories[i]] = new HackyFileOrFolder(directories[i], currentFolder);
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
