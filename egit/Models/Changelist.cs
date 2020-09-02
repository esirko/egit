using System;
using System.Collections.Generic;
using System.Text;

namespace egit.Models
{
    /// <summary>
    /// This is a set of changes in your working directory intended to be submitted as one PR
    /// </summary>
    public class Changelist
    {
        internal Changelist(string d)
        {
            Description = d;
            Files = new List<string>();
        }

        internal string Description;
        internal List<string> Files;
    }

}
