using System;
using System.Collections.Generic;
using System.Text;
using LibGit2Sharp;

namespace egit.Models
{
    [Serializable]
    public class DiffCache
    {
        [Serializable]
        internal class PairOfCommits
        {
            internal PairOfCommits(Commit cc0, Commit cc1)
            {
                c0 = cc0.Id.ToString();
                c1 = cc1.Id.ToString();
            }
            string c0;
            string c1;

            public override string ToString()
            {
                return string.Format(c1 + "-" + c0);
            }

            public override bool Equals(object obj)
            {
                PairOfCommits other = obj as PairOfCommits;
                return c0 == other.c0 && c1 == other.c1;
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

        }

        Dictionary<PairOfCommits, List<string>> Cache = new Dictionary<PairOfCommits, List<string>>();

        public List<string> GetCachedDiff(Commit c0, Commit c1)
        {
            PairOfCommits pair = new PairOfCommits(c0, c1);
            if (Cache.ContainsKey(pair))
            {
                return Cache[pair];
            }
            else
            {
                return null;
            }
        }

        internal void SetCachedDiff(Commit c0, Commit c1, List<string> diffs)
        {
            PairOfCommits pair = new PairOfCommits(c0, c1);
            if (Cache.ContainsKey(pair))
            {
                throw new Exception("Unexpected: you're setting the cached diff but it already is set?");
            }
            else
            {
                Cache[pair] = diffs;
            }
        }

        internal int GetNumEntriesInCache()
        {
            return Cache.Count;
        }

        internal void Clear()
        {
            Cache.Clear();
        }

    }
}
