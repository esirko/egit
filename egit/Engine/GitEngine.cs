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
using Avalonia.Controls;
using egit.Models;
using egit.ViewModels;
using egit.Views;
using LibGit2Sharp;
using MessageBox.Avalonia;

namespace egit.Engine
{
    public class GitEngine : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /*
        internal void RegisterCommitLists(View_CommitList mainCommitList, View_CommitList secondaryCommitList)
        {
            // TODO: I have to make this ViewModelly-class aware of purely View classes, which is not ideal. 
            // I have to do it because DataGrid doesn't support binding to SelectedItems.
            CurrentViewOfCommits.RegisterCommitList(mainCommitList);
            CurrentlyDisplayedFeatureBranch.RegisterCommitList(secondaryCommitList);
        }
        */

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

        public GitEngine()
        {
            CurrentViewOfCommits = new CommitViewEnumerableWrapper(HandleMainSelectedCommitChanged);
            CurrentlyDisplayedFeatureBranch = new CommitViewEnumerableWrapper(HandleSecondarySelectedCommitChanged);
        }

        public CommitViewEnumerableWrapper CurrentViewOfCommits;
        public CommitViewEnumerableWrapper CurrentlyDisplayedFeatureBranch;

        private void HandleMainSelectedCommitChanged(CommitWrapper c1w, CommitWrapper c0w)
        {
            Commit c1 = c1w.Commit;
            ModelTransient.CurrentlySelectedChangelist = -1;
            string commitId1 = c1 != null ? c1.Id.ToString(8) : (c1w.N == -2 ? "[stage]" : (c1w.N == -1 ? "[working]" : "[unknown]"));
            string commitId0 = "";
            if (c0w != null)
            {
                Commit c0 = c0w.Commit;
                commitId0 = c0 != null ? c0.Id.ToString(8) : (c0w.N == -2 ? "[stage]" : (c0w.N == -1 ? "[working]" : "[unknown]"));
                if (c0w.N < c1w.N)
                {
                    string temp = commitId0;
                    commitId0 = commitId1;
                    commitId1 = temp;
                }
            }

            Snapshot oldCommit1 = CurrentDiffCommit1;
            Snapshot oldCommit0 = CurrentDiffCommit0;

            SelectCommitsToDiff(commitId1, commitId0, true);

            bool commitsDidntChange = (oldCommit1.Commit == CurrentDiffCommit1.Commit && oldCommit1.SnapshotType == CurrentDiffCommit1.SnapshotType &&
                oldCommit0.Commit == CurrentDiffCommit0.Commit && oldCommit0.SnapshotType == CurrentDiffCommit0.SnapshotType);

            RefreshListViewCommits2();

            if (!commitsDidntChange)
            {
                RefreshListOfDiffFiles();
            }
        }

        private void HandleSecondarySelectedCommitChanged(CommitWrapper c1w, CommitWrapper c0w)
        {
            if (CurrentDiffCommit1.SnapshotType == SnapshotType.WorkingDirectory)
            {
                int changelistId;
                Int32.TryParse(c1w.Id, out changelistId);
                ModelTransient.CurrentlySelectedChangelist = changelistId;
                RefreshListViewCommits2();
                RefreshListOfDiffFiles();
            }
            else
            {
                Commit c1 = c1w.Commit;
                ModelTransient.CurrentlySelectedChangelist = -1;
                string commitId1 = c1 != null ? c1.Id.ToString(8) : (c1w.N == -2 ? "[stage]" : (c1w.N == -1 ? "[working]" : "[unknown]"));
                string commitId0 = "";
                if (c0w != null)
                {
                    commitId0 = c0w.Commit.Id.ToString(8);
                }

                Snapshot oldCommit1 = CurrentDiffCommit1;
                Snapshot oldCommit0 = CurrentDiffCommit0;

                SelectCommitsToDiff(commitId1, commitId0, false);

                bool commitsDidntChange = (oldCommit1.Commit == CurrentDiffCommit1.Commit && oldCommit1.SnapshotType == CurrentDiffCommit1.SnapshotType &&
                    oldCommit0.Commit == CurrentDiffCommit0.Commit && oldCommit0.SnapshotType == CurrentDiffCommit0.SnapshotType);

                if (!commitsDidntChange)
                {
                    RefreshListOfDiffFiles();
                }
            }
        }

        internal void SelectCommitsToDiff(string commitId1, string commitId0, bool commit0IsInMasterBranch)
        {
            int commitIndex1;

            CurrentDiffCommit1 = GetSnapshotFromCommitId(commitId1, out commitIndex1);
            if (commitId0 == "")
            {
                if (commit0IsInMasterBranch)
                {
                    CurrentDiffCommit0.SnapshotType = SnapshotType.Commit;
                    int commitIndex1Plus1 = Math.Max(0, commitIndex1 + 1);
                    CurrentDiffCommit0.Commit = (commitIndex1Plus1 < MasterBranchCommits.Count) ? MasterBranchCommits[commitIndex1Plus1] : null;
                }
                else
                {
                    CurrentDiffCommit0.Commit = CurrentDiffCommit1.Commit.Parents.FirstOrDefault();
                    if (CurrentDiffCommit0.Commit != null)
                    {
                        CurrentDiffCommit0.SnapshotType = SnapshotType.Commit;
                    }
                }
            }
            else
            {
                int commitIndex0;
                CurrentDiffCommit0 = GetSnapshotFromCommitId(commitId0, out commitIndex0);
            }
        }

        private Snapshot GetSnapshotFromCommitId(string id, out int commitIndex)
        {
            Snapshot snap;
            snap.SnapshotType = SnapshotType.Unknown;
            snap.Commit = null;
            commitIndex = -3;

            if (id == "[stage]")
            {
                snap.SnapshotType = SnapshotType.Stage;
            }
            else if (id == "[working]")
            {
                snap.SnapshotType = SnapshotType.WorkingDirectory;
            }
            else
            {
                commitIndex = MasterBranchCommits.FindIndex(c => c.Id.StartsWith(id));
                if (commitIndex >= 0)
                {
                    snap.SnapshotType = SnapshotType.Commit;
                    snap.Commit = MasterBranchCommits[commitIndex];
                }
                else
                {
                    snap.Commit = Repo.Lookup<Commit>(id);
                    if (snap.Commit != null)
                    {
                        snap.SnapshotType = SnapshotType.Commit;
                    }
                }
            }

            return snap;
        }

        private void RefreshListOfDiffFiles()
        {
            /*
            if (ModelTransient.CurrentlySelectedChangelist < 0)
            {
                CurrentlyDisplayedDiff = Model.GetDiffsBetweenCommits();
            }
            else
            {
                CurrentlyDisplayedDiff = ModelTransient.GetCurrentlySelectedChangelist(Model.CachedWorkingDirectory);
            }

            folvFiles.SetObjects(CurrentlyDisplayedDiff);

            bool useFilter = false;
            string filterPath = "";
            if (toolStripButtonUseFilter.Checked)
            {
                if (textBoxPath.Text.StartsWith(Properties.Settings.Default.LastSelectedLocalRepo))
                {
                    filterPath = textBoxPath.Text.Substring(Properties.Settings.Default.LastSelectedLocalRepo.Length);
                    filterPath = filterPath.Replace('/', '\\').Trim('\\');
                }

                useFilter = filterPath != "";
            }

            folvFiles.UseFiltering = toolStripButtonUseFilter.Checked;

            folvFiles.ModelFilter = useFilter ? new ModelFilter(delegate (object x)
            {
                Model.FileAndStatus fs = (Model.FileAndStatus)x;
                return fs.FileName.StartsWith(filterPath) || (fs.OldFileName != "" && fs.OldFileName.StartsWith(filterPath));
            })
            : null;

            labelCommitInfo.Text = (Model.CurrentDiffCommit1.SnapshotType == Model.SnapshotType.Commit ? Model.CurrentDiffCommit1.Commit.Id.ToString(8) :
                (Model.CurrentDiffCommit1.SnapshotType == Model.SnapshotType.WorkingDirectory ? "[Working directory]" : "[Stage]")) +
                " vs. " + Model.CurrentDiffCommit0.Commit?.Id.ToString(8);

            labelCommitParents.Text = Model.CurrentDiffCommit1.SnapshotType == Model.SnapshotType.Commit ?
                $"{Model.CurrentDiffCommit1.Commit.Id.ToString(8)} : {string.Join(", ", Model.CurrentDiffCommit1.Commit.Parents.Select(p => p.Id.ToString(8)))}"
                : "";

            int numFilesNonFiltered = CurrentlyDisplayedDiff.Count;
            int numFilesFiltered = folvFiles.Items.Count;
            string numTotalFiles = numFilesNonFiltered + " file" + (numFilesNonFiltered != 1 ? "s" : "");
            if (numFilesNonFiltered == numFilesFiltered)
            {
                labelCommitNumFiles.ForeColor = Color.Black;
                labelCommitNumFiles.Text = numTotalFiles;
            }
            else
            {
                labelCommitNumFiles.ForeColor = Color.Red;
                labelCommitNumFiles.Text = numFilesFiltered + " of " + numTotalFiles;
            }

            labelCommitMessage.Text = (Model.CurrentDiffCommit1.SnapshotType == Model.SnapshotType.Commit) ? Model.CurrentDiffCommit1.Commit.Message : "";
            */

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
                    MasterBranchCommits.Clear();
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
                        MessageBoxManager.GetMessageBoxStandardWindow("Bad repo", $"Not a valid repo: {CurrentRepoPath}").Show();
                    }
                }

                delayTime /= 2;
                Task t = new Task(async () => { await DoStuffAsync(); });
                t.Start();

                // TODO: we probably want to use the same code here for traversing a _different_ branch in the _same_ repo. That's what the `newRepo` boolean
                // was in the old code. This propagated to the variables NeedToReloadDiffCache.
                Task t2 = new Task(async () => { await RefreshComboBoxBranchesAsync(); });
                Task t3 = new Task(async () => { await TraverseHeadBranchAsync(); });
                Task t4 = new Task(async () => { await AnalyzeHeadBranchCommitsAsync(); });
                Task t5 = new Task(async () => { await DoGitStatusAsync(); });
                Task t6 = new Task(async () => { await TraverseHistoryFSAsync(); });
                t2.Start();
                t3.Start();
                t4.Start();
                t5.Start();
                t6.Start();
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

            CurrentViewOfCommits.Commits.SetLastStageAndWorkingDirectoryRefreshTime(DateTime.MinValue);

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

            ModelTransient.RefreshChangelistsFromWorkingDirectory(CachedWorkingDirectory);
            CurrentViewOfCommits.Commits.SetLastStageAndWorkingDirectoryRefreshTime(LastStageAndWorkingDirectoryRefreshTime);
            if (CurrentDiffCommit1.SnapshotType == SnapshotType.WorkingDirectory || CurrentDiffCommit1.SnapshotType == SnapshotType.Stage)
            {
                RefreshListViewCommits2();
                RefreshListOfDiffFiles();
            }

            LastStageAndWorkingDirectoryRefreshTime = DateTime.Now;
        }

        private async Task TraverseHeadBranchAsync()
        {
            if (CurrentSelectedBranch == null)
            {
                return;
            }

            CurrentlyTraversingHeadBranch = true;
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
            CurrentlyTraversingHeadBranch = false;
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

                if (!CurrentlyTraversingHeadBranch && NumCommitsAnalyzed == MasterBranchCommits.Count - 1)
                {
                    break;
                }
                // TODO: use cancellation token here and everywhere there's a Task.Yield
                await Task.Yield();
            }

            CommitAnalyzerStopwatch.Stop();
        }

        private Task TraverseHistoryFSAsync()
        {
            /*
            List<Tuple<TreeNode, TreeNode>> pendingTreeNodeAdditions = new List<Tuple<TreeNode, TreeNode>>();
            List<string> pendingUserAdditions = new List<string>();
            TraverseHistoryFS(rootNode, HistoryFS.BaseFolder, pendingTreeNodeAdditions);
            */

            return Task.CompletedTask;
        }

        /*
        private void TraverseHistoryFS(TreeNode node, HackyFileOrFolder hackyFileOrFolder, List<Tuple<TreeNode, TreeNode>> pendingTreeNodeAdditions)
        {
            // This now happens on a worker thread.
            for (int i = 0; i < hackyFileOrFolder.Entries.Count; i++)
            {
                string path = hackyFileOrFolder.Entries.Keys[i];
                TreeNode[] foundNodes = node.Nodes.Find(path, false);
                TreeNode subnode = foundNodes.Length > 0 ? foundNodes[0] : null;
                bool preexisting = subnode != null;

                if (!preexisting)
                {
                    subnode = new TreeNode(path);
                    subnode.Tag = hackyFileOrFolder.Entries[path]; // I get an exception on this line sometimes
                    subnode.Name = path;
                }

                HackyFileOrFolder next = hackyFileOrFolder.Entries[path];

                TraverseHistoryFS(subnode, next, pendingTreeNodeAdditions);

                if (!preexisting)
                {
                    pendingTreeNodeAdditions.Add(new Tuple<TreeNode, TreeNode>(node, subnode));
                }
            }
        }
*/



        private void FlushPendingMasterCommits(ref int nn, ref int k, ref List<Commit> pendingMasterCommits)
        {
            for (int i = 0; i < pendingMasterCommits.Count; i++)
            {
                Commit c = pendingMasterCommits[i];
                MasterBranchCommits.Add(c);

                CurrentViewOfCommits.Commits = new CommitViewEnumerable(MasterBranchCommits, true);

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

        private void RefreshListViewCommits2()
        {
            if (CurrentDiffCommit1.SnapshotType == SnapshotType.WorkingDirectory)
            {
                CurrentlyDisplayedFeatureBranch.Commits = new CommitViewEnumerable(ModelTransient.Changelists);
            }
            else
            {
                CurrentlyDisplayedFeatureBranch.Commits = new CommitViewEnumerable(GetFeatureBranchCommits());
            }
        }

        internal List<Commit> GetFeatureBranchCommits()
        {
            List<Commit> featureBranchCommits = new List<Commit>();

            if (CurrentDiffCommit1.SnapshotType == SnapshotType.Commit)
            {
                featureBranchCommits.Add(CurrentDiffCommit1.Commit);

                int i = 0;
                while (true)
                {
                    List<Commit> parents = featureBranchCommits[i].Parents.ToList();
                    if (parents.Count == 2)
                    {
                        // Expect one of the parents to be on the master branch, and this is a remerge on the feature branch
                        bool m0 = MasterBranchCommits.Contains(parents[0]);
                        bool m1 = MasterBranchCommits.Contains(parents[1]);
                        if (m0 && !m1)
                        {
                            featureBranchCommits.Add(parents[1]);
                        }
                        else if (m1 && !m0)
                        {
                            featureBranchCommits.Add(parents[0]);
                        }
                        else
                        {
                            //thereWasAProblem = true;
                            Console.WriteLine("Merge of two non-master branches... this means we can't present one of the branches yet");
                            featureBranchCommits.Add(parents[0]);
                            //break;
                        }
                    }
                    else
                    {
                        if (parents.Count == 0)
                        {
                            if (MasterBranchCommits.Last() == CurrentDiffCommit1.Commit)
                            {
                                break;
                            }
                            else
                            {
                                MessageBoxManager.GetMessageBoxStandardWindow("", "Got to here, avoiding a crash? Need to investigate this. This just happened twice on 7-12 and seems to have just started... The next MessageBox will have more info.").Show();
                                MessageBoxManager.GetMessageBoxStandardWindow("", "May be because there are zero parents to featureBranch Commit " + i + " " + featureBranchCommits[i].Id).Show();
                            }
                            break;
                        }
                        if (parents[0] == null)
                        {
                            break; // avoid a crash if you hit the end of the line
                        }
                        if (MasterBranchCommits.Contains(parents[0]))
                        {
                            break;
                        }
                        else
                        {
                            featureBranchCommits.Add(parents[0]);
                        }
                    }
                    i++;

                    if (featureBranchCommits.Count > 100)
                    {
                        break; // TODO: investigate why sometimes the feature branch traversal is unbounded. 
                    }
                }
            }
            else if (CurrentDiffCommit1.SnapshotType == SnapshotType.WorkingDirectory)
            {

            }

            return featureBranchCommits;
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
        public FileSystem HistoryFS = new FileSystem();
        public FileSystem UserFS = new FileSystem(); // hijacking the FileSystem class to store user commits

        internal Snapshot CurrentDiffCommit0;
        internal Snapshot CurrentDiffCommit1;
        internal ModelTransient ModelTransient = new ModelTransient();


        internal Repository Repo;

        private static GitEngine _Singleton;
        private string CurrentRepoPath;
        int delayTime = 2000;
        private int _Counter;
        private ViewModel_RepoInfo RepoInfo;

        bool CurrentlyTraversingHeadBranch = false;

    }
}
