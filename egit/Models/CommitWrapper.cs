using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using LibGit2Sharp;

namespace egit.Models
{
    public class CommitWrapper
    {
        public int SelfIndex;
        public Commit Commit;
        public Changelist Changelist;
        public DateTime LastStageAndWorkingDirectoryRefreshTime = DateTime.MinValue; // This is only used for the stage and working directory, which seems like a waste for all the other commits, but oh well

        public CommitWrapper(int selfIndex, Changelist c)
        {
            Commit = null;
            Changelist = c;
            SelfIndex = selfIndex;
        }

        public CommitWrapper(int selfIndex, DateTime? stageAndWorkingDirectoryRefreshTime = null)
        {
            Commit = null;
            Changelist = null;
            SelfIndex = selfIndex;
            if (stageAndWorkingDirectoryRefreshTime != null)
            {
                LastStageAndWorkingDirectoryRefreshTime = stageAndWorkingDirectoryRefreshTime.Value;
            }
        }

        public CommitWrapper(int selfIndex, Commit c)
        {
            Commit = c;
            Changelist = null;
            SelfIndex = selfIndex;
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
                    return (LastStageAndWorkingDirectoryRefreshTime != DateTime.MinValue) ? LastStageAndWorkingDirectoryRefreshTime.ToString("yyyy-MM-dd hh:mm:ss tt") : "[populating...]";
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
}
