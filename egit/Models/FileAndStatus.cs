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
        public ChangeKind Status;
        public string FileName;
        public string OldFileName;
    }
}
