using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MessageBox.Avalonia;

namespace egit.Models
{
    class ModelTransient
    {
        internal List<Changelist> Changelists = new List<Changelist>();
        Dictionary<string, int> FileToChangelistMapping = new Dictionary<string, int>();

        internal int CurrentlySelectedChangelist = -1; // -1 means no changelist is selected, 0 is the default changelist

        internal ModelTransient()
        {
            Changelists.Add(new Changelist("<default>"));
        }

        internal int GetChangelistForFile(string filename)
        {
            if (FileToChangelistMapping.ContainsKey(filename))
            {
                return FileToChangelistMapping[filename];
            }
            return -1;
        }

        internal void MoveFilesToChangelist(int changelistId, List<string> files, string changelistDescription = null)
        {
            List<bool> fileIsInWorkingSet = new List<bool>();
            for (int i = 0; i < files.Count; i++)
            {
                fileIsInWorkingSet.Add(false);
            }

            for (int i = 0; i < files.Count; i++)
            {
                if (FileToChangelistMapping.ContainsKey(files[i]))
                {
                    int j = FileToChangelistMapping[files[i]];
                    Changelists[j].Files.Remove(files[i]);
                    fileIsInWorkingSet[i] = true;
                }
            }

            if (changelistId < 0)
            {
                changelistId = Changelists.Count;
                Changelists.Add(new Changelist(changelistDescription));
            }
            else if (changelistDescription != null)
            {
                Changelists[changelistId].Description = changelistDescription; // not sure if it makes sense to offer the option to modify the description at the same time you are adding files to the changelist...
            }

            for (int i = 0; i < files.Count; i++)
            {
                if (fileIsInWorkingSet[i])
                {
                    Changelists[changelistId].Files.Add(files[i]);
                    FileToChangelistMapping[files[i]] = changelistId;
                }
            }
        }

        internal void DeleteChangelists(List<int> changelistIndices)
        {
            changelistIndices.Sort();
            for (int i = changelistIndices.Count - 1; i >= 0; i--)
            {
                int changelistId = changelistIndices[i];
                MoveFilesToChangelist(0, Changelists[changelistId].Files);
                Changelists.RemoveAt(changelistIndices[i]);
            }
            CurrentlySelectedChangelist = 0;
        }


        internal List<FileAndStatus> GetCurrentlySelectedChangelist(List<FileAndStatus> cachedWorkingDirectory)
        {
            List<FileAndStatus> changelist = new List<FileAndStatus>();
            if (CurrentlySelectedChangelist >= 0)
            {
                if (CurrentlySelectedChangelist >= Changelists.Count)
                {
                    MessageBoxManager.GetMessageBoxStandardWindow("", "Invalid index. Unexpected after I fixed a bug 2016-09-13.. if it happens again, what happened?").Show();
                    return changelist;
                }
                List<string> listOfFiles = Changelists[CurrentlySelectedChangelist].Files;
                for (int i = 0; i < listOfFiles.Count; i++)
                {
                    FileAndStatus fs = cachedWorkingDirectory.Find(c => c.FileName == listOfFiles[i]);
                    if (fs != null)
                    {
                        changelist.Add(fs);
                    }
                }
            }
            else
            {
                MessageBoxManager.GetMessageBoxStandardWindow("", "Unexpected, you're calling this with CurrentlySelectedChangelist < 0").Show();
                changelist.AddRange(cachedWorkingDirectory);
            }
            return changelist;
        }

        internal void RefreshChangelistsFromWorkingDirectory(List<FileAndStatus> cachedWorkingDirectory)
        {
            // Put everything in default!!!
            FileToChangelistMapping.Clear();
            Changelists.Clear();
            Changelists.Add(new Changelist("<default>"));
            for (int i = 0; i < cachedWorkingDirectory.Count; i++)
            {
                Changelists[0].Files.Add(cachedWorkingDirectory[i].FileName);
                FileToChangelistMapping[cachedWorkingDirectory[i].FileName] = 0;
            }

            // Then transition things to other changelists if they have been saved...
            string filename = GetSavedChangelistsFileName();
            if (File.Exists(filename))
            {
                List<string> lines = File.ReadAllLines(GetSavedChangelistsFileName()).ToList();
                int k = 0;
                while (k < lines.Count)
                {
                    int spacepos = lines[k].IndexOf(" ");
                    int numFiles = Int32.Parse(lines[k].Substring(0, spacepos));
                    string description = lines[k].Substring(spacepos + 1);
                    MoveFilesToChangelist(-1, lines.GetRange(k + 1, numFiles), description);
                    k += 1 + numFiles;
                }
            }
        }

        internal void SaveChangelistsToDisk()
        {
            List<string> lines = new List<string>();
            for (int i = 1; i < Changelists.Count; i++)
            {
                lines.Add(Changelists[i].Files.Count + " " + Changelists[i].Description);
                for (int j = 0; j < Changelists[i].Files.Count; j++)
                {
                    lines.Add(Changelists[i].Files[j]);
                }
            }

            File.WriteAllLines(GetSavedChangelistsFileName(), lines.ToArray());
        }

        private string GetSavedChangelistsFileName()
        {
            string reducedProjectName = Settings.Default.LastSelectedLocalRepo;
            reducedProjectName = reducedProjectName.Replace('\\', '.').Replace('/', '.').Replace(':', '-');
            return Settings.Default.DiffCacheDir + @"\_Changelists_" + reducedProjectName + ".txt";
        }

    }
}
