using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using LibGit2Sharp;

namespace egit.Models
{
    public class CommitWrapper
    {
        int SelfIndex;
        CommitViewEnumerable HostCommitViewEnumerable;
        public Commit Commit;
        public Changelist Changelist;

        public CommitWrapper(int selfIndex, Changelist c, CommitViewEnumerable hostEnumerable)
        {
            Commit = null;
            Changelist = c;
            SelfIndex = selfIndex;
            HostCommitViewEnumerable = hostEnumerable;
        }

        public CommitWrapper(int selfIndex, CommitViewEnumerable hostEnumerable)
        {
            Commit = null;
            Changelist = null;
            SelfIndex = selfIndex;
            HostCommitViewEnumerable = hostEnumerable;
        }

        public CommitWrapper(int selfIndex, Commit c, CommitViewEnumerable hostEnumerable)
        {
            Commit = c;
            Changelist = null;
            SelfIndex = selfIndex;
            HostCommitViewEnumerable = hostEnumerable;
        }

        public string Id
        {
            get
            {
                if (Commit != null)
                {
                    return Commit.Id.ToString(8);
                }
                else if (Changelist != null)
                {
                    return SelfIndex.ToString();
                }
                else
                {
                    return SelfIndex == -2 ? "[stage]" : (SelfIndex == -1 ? "[working]" : "[unknown]");
                }
            }
        }
        public int N
        {
            get { return SelfIndex; }
        }

        public string Date
        {
            get
            {
                if (Commit != null)
                {
                    return Commit.Committer.When.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss tt");
                }
                else if (Changelist != null)
                {
                    return "[pending]";
                }
                else
                {
                    return (HostCommitViewEnumerable.LastStageAndWorkingDirectoryRefreshTime != DateTime.MinValue) ? HostCommitViewEnumerable.LastStageAndWorkingDirectoryRefreshTime.ToString("yyyy-MM-dd hh:mm:ss tt") : "[populating...]";
                }
            }
        }
        public string Author
        {
            get
            {
                if (Commit != null)
                {
                    return Commit.Author.Name;
                }
                else if (Changelist != null)
                {
                    return "[self]";
                }
                else
                {
                    return SelfIndex == -2 || SelfIndex == -1 ? "[self]" : "[unknown]";
                }
            }
        }
        public string Message
        {
            get
            {
                if (Commit != null)
                {
                    return Commit.MessageShort;
                }
                else if (Changelist != null)
                {
                    return Changelist.Description + " (" + Changelist.Files.Count + ")";
                }
                else
                {
                    return SelfIndex == -2 ? "[stage]" : (SelfIndex == -1 ? "[working]" : "[unknown]");
                }
            }
        }

    }

    // IEnumerable stuff from https://msdn.microsoft.com/en-us/library/s793z9y2(v=vs.110).aspx
    public class CommitViewEnumerable : IEnumerable<CommitWrapper>
    {
        List<Commit> GitCommits;
        List<Changelist> PendingChangelists;
        bool ShowWorkingDirectory;
        public DateTime LastStageAndWorkingDirectoryRefreshTime;

        public CommitViewEnumerable(List<Changelist> changelists)
        {
            GitCommits = null;
            PendingChangelists = changelists;
            ShowWorkingDirectory = false;
        }

        public CommitViewEnumerable(List<Commit> commits, bool showWorkingDirectory = false, DateTime? lastWDRefreshTime = null)
        {
            GitCommits = commits;
            PendingChangelists = null;
            ShowWorkingDirectory = showWorkingDirectory;
            LastStageAndWorkingDirectoryRefreshTime = lastWDRefreshTime.HasValue ? lastWDRefreshTime.Value : DateTime.MinValue;
        }

        public IEnumerator<CommitWrapper> GetEnumerator()
        {
            return new CommitViewEnumerator(this, GitCommits, PendingChangelists, ShowWorkingDirectory);
        }

        private IEnumerator GetEnumerator1()
        {
            return this.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator1();
        }

        public int Count
        {
            get { return GitCommits.Count + (ShowWorkingDirectory ? 2 : 0); }
        }

        internal void SetLastStageAndWorkingDirectoryRefreshTime(DateTime lastStageAndWorkingDirectoryRefreshTime)
        {
            LastStageAndWorkingDirectoryRefreshTime = lastStageAndWorkingDirectoryRefreshTime;
        }
    }

    internal class CommitViewEnumerator : IEnumerator<CommitWrapper>
    {
        CommitViewEnumerable HostCommitViewEnumerable;
        List<Commit> GitCommits;
        List<Changelist> PendingChangelists;
        bool ShowWorkingDirectory;
        int CurrentIndex;

        public CommitViewEnumerator(CommitViewEnumerable hostCommitViewEnumerable, List<Commit> gitCommits, List<Changelist> changelists, bool showWorkingDirectory)
        {
            HostCommitViewEnumerable = hostCommitViewEnumerable;
            GitCommits = gitCommits;
            PendingChangelists = changelists;
            ShowWorkingDirectory = showWorkingDirectory;
            CurrentIndex = ShowWorkingDirectory ? -3 : -1;
        }
        public CommitWrapper Current
        {
            get
            {
                if (ShowWorkingDirectory && CurrentIndex < 0)
                {
                    return new CommitWrapper(CurrentIndex, HostCommitViewEnumerable);
                }

                if (GitCommits == null)
                {
                    if (PendingChangelists == null || CurrentIndex >= PendingChangelists.Count)
                    {
                        throw new InvalidOperationException();
                    }
                    return new CommitWrapper(CurrentIndex, PendingChangelists[CurrentIndex], HostCommitViewEnumerable);
                }

                if (CurrentIndex >= GitCommits.Count)
                {
                    throw new InvalidOperationException();
                }

                return new CommitWrapper(CurrentIndex, GitCommits[CurrentIndex], HostCommitViewEnumerable);
            }
        }

        private object Current1
        {
            get { return this.Current; }
        }

        object IEnumerator.Current
        {
            get { return Current1; }
        }

        public void Dispose()
        {
            GitCommits = null;
            PendingChangelists = null;
            ShowWorkingDirectory = false;
            CurrentIndex = -1;
        }

        public bool MoveNext()
        {
            CurrentIndex++;
            if (GitCommits == null)
            {
                if (PendingChangelists == null || CurrentIndex >= PendingChangelists.Count)
                {
                    return false;
                }
            }
            else
            {
                if (CurrentIndex >= GitCommits.Count)
                {
                    return false;
                }
            }
            return true;
        }

        public void Reset()
        {
            CurrentIndex = ShowWorkingDirectory ? -3 : -1;
        }
    }
}
