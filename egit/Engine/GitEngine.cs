using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using egit.Models;
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
                        Repo = new Repository(CurrentRepoPath); // TODO: this is still in the synchronous path. Should it be asyncified?
                        CurrentSelectedBranch = Repo.Head;
                    }
                    else
                    {
                        MessageBoxManager.GetMessageBoxStandardWindow("Bad repo", $"Not a valid repo: {CurrentRepoPath}");
                    }
                }

                delayTime /= 2;
                Task t = new Task(async () => { await DoStuffAsync(); });
                t.Start();

                Task t2 = new Task(async () => { await RefreshComboBoxBranchesAsync(); });
                Task t3 = new Task(async () => { await TraverseHeadBranchAsync(); });
                Task t4 = new Task(async () => { await AnalyzeHeadBranchCommitsAsync(); });
                Task t5 = new Task(async () => { await DoGitStatusAsync(); });
                t2.Start();
                t3.Start();
                t4.Start();
                t5.Start();
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

        private async Task DoGitStatusAsync()
        {
            CachedStage.Clear();
            CachedWorkingDirectory.Clear();

            foreach (var item in Repo.RetrieveStatus())
            {
                if (item.State != FileStatus.Ignored)
                {
                    Console.WriteLine(item.State + " " + item.FilePath);
                    string originalFileName = "";

                    ChangeKind reducedState = ChangeKind.Ignored;
                    if ((item.State & FileStatus.NewInWorkdir) > 0) reducedState = ChangeKind.Added;
                    else if ((item.State & FileStatus.DeletedFromWorkdir) > 0) reducedState = ChangeKind.Deleted;
                    else if ((item.State & FileStatus.ModifiedInWorkdir) > 0) reducedState = ChangeKind.Modified;
                    else if ((item.State & FileStatus.RenamedInWorkdir) > 0)
                    {
                        reducedState = ChangeKind.Renamed;
                        originalFileName = item.IndexToWorkDirRenameDetails.OldFilePath; // This may be wrong... it's untested..
                    }
                    else if ((item.State & FileStatus.TypeChangeInWorkdir) > 0) reducedState = ChangeKind.TypeChanged;
                    if (reducedState != ChangeKind.Ignored)
                    {
                        CachedWorkingDirectory.Add(new FileAndStatus(reducedState, item.FilePath, originalFileName));
                    }

                    reducedState = ChangeKind.Ignored;
                    if ((item.State & FileStatus.NewInIndex) > 0) reducedState = ChangeKind.Added;
                    else if ((item.State & FileStatus.DeletedFromIndex) > 0) reducedState = ChangeKind.Deleted;
                    else if ((item.State & FileStatus.ModifiedInIndex) > 0) reducedState = ChangeKind.Modified;
                    else if ((item.State & FileStatus.RenamedInIndex) > 0)
                    {
                        reducedState = ChangeKind.Renamed;
                        originalFileName = item.HeadToIndexRenameDetails.OldFilePath;
                    }
                    else if ((item.State & FileStatus.TypeChangeInIndex) > 0) reducedState = ChangeKind.TypeChanged;
                    if (reducedState != ChangeKind.Ignored)
                    {
                        CachedStage.Add(new FileAndStatus(reducedState, item.FilePath, originalFileName));
                    }
                }
                await Task.Yield();
            }

            LastStageAndWorkingDirectoryRefreshTime = DateTime.Now;
        }

        private async Task TraverseHeadBranchAsync()
        {
            if (CurrentSelectedBranch == null)
            {
                return;
            }

            int k = 0;
            int nn = 0;

            Commit nextPendingCommitInMaster = CurrentSelectedBranch.Commits.First();
            Commit lastPendingMasterCommitTraversed = null;
            Commit lastDefiniteMasterCommitTraversed = null;
            int numConsecutiveProblems = 0;
            List<Commit> pendingMasterCommits = new List<Commit>();

            List<Commit> pendingTouchedCommits = new List<Commit>();
            pendingTouchedCommits.Add(nextPendingCommitInMaster);

            List<Commit> orphanedCommits = new List<Commit>();
            bool TraversalMethod2 = true;

            // Note: foreach (LogEntry le in Repo.Commits.QueryBy("VssfSdkSample/")) .. only works in prerelease version and too slow .. and doesn't even return the PR commits..
            foreach (Commit c in CurrentSelectedBranch.Commits)
            {
                if (TraversalMethod2)
                {
                    if (c == nextPendingCommitInMaster)
                    {
                        pendingMasterCommits.Add(c);
                        lastPendingMasterCommitTraversed = c;
                        nextPendingCommitInMaster = c.Parents.Count() > 0 ? c.Parents.First() : null;
                        while (orphanedCommits.Contains(nextPendingCommitInMaster))
                        {
                            orphanedCommits.Remove(nextPendingCommitInMaster);
                            pendingMasterCommits.Add(nextPendingCommitInMaster);
                            lastPendingMasterCommitTraversed = nextPendingCommitInMaster;
                            nextPendingCommitInMaster = nextPendingCommitInMaster.Parents.FirstOrDefault();
                        }

                        FlushPendingMasterCommits(ref nn, ref k, ref pendingMasterCommits);
                        lastDefiniteMasterCommitTraversed = c;
                        numConsecutiveProblems = 0;
                        pendingTouchedCommits.Clear();
                        pendingTouchedCommits.Add(c);
                    }
                    else
                    {
                        orphanedCommits.Add(c);
                    }
                }
                else
                {
                    if (c == nextPendingCommitInMaster)
                    {
                        pendingMasterCommits.Add(c);
                        lastPendingMasterCommitTraversed = c;
                        nextPendingCommitInMaster = c.Parents.Count() > 0 ? c.Parents.First() : null;

                        if (CommitHasPRSignature(c) || k == 0)
                        {
                            FlushPendingMasterCommits(ref nn, ref k, ref pendingMasterCommits);
                            lastDefiniteMasterCommitTraversed = c;
                            numConsecutiveProblems = 0;
                            pendingTouchedCommits.Clear();
                            pendingTouchedCommits.Add(c);
                        }
                    }
                    else if (CommitHasPRSignature(c))
                    {
                        // Only consider it a problem if this is a parent of a previously-traversed commit. For example, it is NOT a problem when we encounter c6a59e13 because it has no children and it's on a different branch.
                        if (pendingTouchedCommits.Contains(c))
                        {
                            numConsecutiveProblems++;

                            if (numConsecutiveProblems > Settings.Default.MaxConsecutiveProblems)
                            {
                                // Go back to lastDefiniteMasterCommitTraversed and traverse the path to c.
                                pendingMasterCommits.Clear();
                                lastPendingMasterCommitTraversed = lastDefiniteMasterCommitTraversed;
                                while (lastPendingMasterCommitTraversed != c)
                                {
                                    lastPendingMasterCommitTraversed = TraverseTreeFromCommitToCommitAndReturnFirstStep(lastPendingMasterCommitTraversed, c);
                                    pendingMasterCommits.Add(lastPendingMasterCommitTraversed);
                                }

                                nextPendingCommitInMaster = c.Parents.First();

                                FlushPendingMasterCommits(ref nn, ref k, ref pendingMasterCommits);
                                lastDefiniteMasterCommitTraversed = c;
                                numConsecutiveProblems = 0;
                            }
                        }
                    }

                    if (pendingTouchedCommits.Contains(c))
                    {
                        foreach (Commit p in c.Parents)
                        {
                            pendingTouchedCommits.Add(p);
                        }
                    }

                    k++;
                }
                await Task.Yield();
            }

            FlushPendingMasterCommits(ref nn, ref k, ref pendingMasterCommits);
        }

        private async Task AnalyzeHeadBranchCommitsAsync()
        {
            if (NeedToReloadDiffCache)
            {
                DiffCache.Clear();

                if (!string.IsNullOrEmpty(CurrentRepoPath))
                {
                    string diffCacheFile = GetDiffCacheFileName(CurrentRepoPath);
                    if (File.Exists(diffCacheFile))
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        IFormatter formatter = new BinaryFormatter();
                        Stream stream = new FileStream(diffCacheFile, FileMode.Open, FileAccess.Read, FileShare.None);
                        DiffCache = (DiffCache)formatter.Deserialize(stream);
                        stream.Close();
                        sw.Stop();
                        Console.WriteLine("Loaded DiffCache: " + DiffCache.GetNumEntriesInCache() + " entries");
                        Console.WriteLine("Loading DiffCache took " + sw.Elapsed + " seconds");
                    }
                }
            }

            int i0 = 0;
            while (true)
            {
                int n = MasterBranchCommits.Count;

                if (i0 >= n - 1 && CommitAnalyzerStopwatch.IsRunning) CommitAnalyzerStopwatch.Stop();
                else if (i0 < n - 1 && !CommitAnalyzerStopwatch.IsRunning) CommitAnalyzerStopwatch.Start();

                for (int i = i0; i < n - 1; i++)
                {
                    PopulateHistoryFileSystemWithCommit(MasterBranchCommits[i], MasterBranchCommits[i + 1]);
                    NumCommitsAnalyzed = i + 1;
                    // TODO: use cancellation token here
                }

                i0 = NumCommitsAnalyzed;

                if (NumCommitsAnalyzed == MasterBranchCommits.Count - 1) // TODO: what did this mean? && !Model.MasterBranchTraverser_IsBusy)
                {
                    break;
                }
                // TODO: use cancellation token here and everywhere there's a Task.Yield
                await Task.Yield();
            }

            CommitAnalyzerStopwatch.Stop();
        }


        private void FlushPendingMasterCommits(ref int nn, ref int k, ref List<Commit> pendingMasterCommits)
        {
            for (int i = 0; i < pendingMasterCommits.Count; i++)
            {
                Commit c = pendingMasterCommits[i];
                MasterBranchCommits.Add(c);

                nn++;
            }
            pendingMasterCommits.Clear();
        }


        private Commit TraverseTreeFromCommitToCommitAndReturnFirstStep(Commit c, Commit destination)
        {
            // Do a breadth-first search for the signature for pull request merges, and go down the branch that hits one first.
            // This assumes that all (or almost all) master branch submissions have that format...

            // TODO: you can avoid the infinite loop by detecting when your commits start to have timestamps less than the destination's

            List<Commit> CommitsByBreadth = new List<Commit>();
            Dictionary<Commit, Commit> MapOfParentToChild = new Dictionary<Commit, Commit>();
            CommitsByBreadth.Add(c);
            int i0 = 0;
            int n = 1;

            Commit nextPR = null;
            while (nextPR == null)
            {
                if (i0 == n) // || n > 4000)
                {
                    throw new Exception("Preventing infinite loop");
                }
                for (int i = i0; i < n; i++)
                {
                    foreach (Commit p in CommitsByBreadth[i].Parents)
                    {
                        if ((destination == null && CommitHasPRSignature(p)) || p == destination)
                        {
                            if (nextPR != null)
                            {
                                throw new Exception("Two parents that match the pull request?");
                            }
                            nextPR = p;
                        }
                        if (!MapOfParentToChild.ContainsKey(p))
                        {
                            MapOfParentToChild.Add(p, CommitsByBreadth[i]);
                            CommitsByBreadth.Add(p);
                        }
                    }
                    if (nextPR != null)
                    {
                        break;
                    }
                }
                i0 = n;
                n = CommitsByBreadth.Count;
            }

            // Walk back up the chain I just constructed to find c's parent that leads to this one.
            Commit child = MapOfParentToChild[nextPR];
            while (child != c)
            {
                nextPR = child; // Now it refers to some commit that may not be a PR proper, but leads to a PR
                child = MapOfParentToChild[nextPR];
            }
            return nextPR;
        }

        private bool CommitHasPRSignature(Commit c)
        {
            return PRRegex1.Match(c.MessageShort).Success || (PRRegex2.Match(c.MessageShort).Success && c.Parents.Count() == 2)
                || (PRRegex3.Match(c.MessageShort).Success && c.Parents.Count() == 2) ||
                PRRegex4.Match(c.MessageShort).Success ||
                PRRegex5.Match(c.MessageShort).Success;
        }

        private void PopulateHistoryFileSystemWithCommit(Commit c1, Commit c0)
        {
            List<string> diffs = DiffCache.GetCachedDiff(c0, c1);
            if (diffs == null)
            {
                diffs = new List<string>();
                TreeChanges changes = Repo.Diff.Compare<TreeChanges>(c0.Tree, c1.Tree);
                foreach (TreeEntryChanges c in changes)
                {
                    diffs.Add(c.Path);
                }
                DiffCache.SetCachedDiff(c0, c1, diffs);
            }

            for (int i = 0; i < diffs.Count; i++)
            {
                HistoryFS.PopulateWithFileAndCommitPair(diffs[i], c1);
            }

            UserFS.PopulateWithFileAndCommitPair(c1.Author.Name, c1);
        }

        private string GetDiffCacheFileName(string lastSelectedLocalRepo)
        {
            string reducedProjectName = lastSelectedLocalRepo.Replace('\\', '.').Replace('/', '.').Replace(':', '-');
            return Settings.Default.DiffCacheDir + @"\_DiffCache_" + reducedProjectName + ".bin";
        }

        internal void SaveDiffCache()
        {
            string diffCacheFile = GetDiffCacheFileName(CurrentRepoPath);

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(diffCacheFile, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, DiffCache);
            stream.Close();
        }




        internal Branch CurrentSelectedBranch = null;
        internal int IndexOfHeadBranch = -1; // TODO: refactor this, shouldn't store as int


        internal DateTime LastStageAndWorkingDirectoryRefreshTime = DateTime.MinValue;
        internal List<FileAndStatus> CachedStage = new List<FileAndStatus>();
        internal List<FileAndStatus> CachedWorkingDirectory = new List<FileAndStatus>();


        internal List<Commit> MasterBranchCommits = new List<Commit>();
        Regex PRRegex1 = new Regex("^(Merged )?PR \\d+:");
        Regex PRRegex2 = new Regex("^Merge pull request \\d+ from .* into master$"); // for mseng.visualstudio.com
        Regex PRRegex3 = new Regex("^Merge PR \\d+ into master"); // For DevFabric
        Regex PRRegex4 = new Regex("^Merge pull request"); // For office-ui-fabric-core
        Regex PRRegex5 = new Regex("\\(#\\d+\\)$"); // For github?


        DiffCache DiffCache = new DiffCache();
        internal bool NeedToReloadDiffCache = false;
        internal Stopwatch CommitAnalyzerStopwatch = new Stopwatch();
        internal int NumCommitsAnalyzed = 0;
        internal FileSystem HistoryFS = new FileSystem();
        internal FileSystem UserFS = new FileSystem(); // hijacking the FileSystem class to store user commits


        internal Repository Repo;

        private static GitEngine _Singleton;
        private string CurrentRepoPath;
        int delayTime = 2000;
        private int _Counter;
        private ViewModel_RepoInfo RepoInfo;

    }
}
