using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Media;
using LibGit2Sharp;

namespace egit.Models
{
    public class BranchWrapper
    {
        public Branch Branch;

        public BranchWrapper(Branch b)
        {
            Branch = b;
        }

        public string FriendlyName
        {
            get
            {
                return Branch?.FriendlyName ?? "Enumerating branches...";
            }
        }

        public FontWeight BranchFontWeight // TODO: what's the proper way to have the View know about FontWeight but this model know only about IsCurrentRepositoryHead?
        {
            get
            {
                return (Branch != null && Branch.IsCurrentRepositoryHead) ? FontWeight.Bold : FontWeight.Normal;
            }
        }

    }
}
