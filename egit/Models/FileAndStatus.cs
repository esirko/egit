using System;
using System.Collections.Generic;
using System.Text;
using LibGit2Sharp;

namespace egit.Models
{
    public class FileAndStatus
    {
        public FileAndStatus(ChangeKind s, string f, string oldFileName)
        {
            Status = s;
            FileName = f;
            OldFileName = oldFileName;
        }
        public ChangeKind Status { get; private set; } // private set is necessary so it's not editable in the UI
        public string FileName { get; }
        public string OldFileName { get; }

        public void SetStatus(ChangeKind s) { Status = s; }
    }
}
