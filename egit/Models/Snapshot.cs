using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibGit2Sharp;

namespace egit.Models
{
    public enum SnapshotType
    {
        Unknown,
        Stage,
        WorkingDirectory,
        Commit,
    }

    public struct Snapshot
    {
        public Snapshot(SnapshotType t, Commit c)
        {
            SnapshotType = t;
            Commit = c;
        }

        public SnapshotType SnapshotType;
        public Commit Commit; // only matters if SnapshotType == Commit.

        internal string GetFileToDiffAndCreateIfNecessary(Repository repo, string relFilePath)
        {
            string filename = "";
            string tempDiffFileDir = Settings.Default.TempDiffFileDir;
            switch (SnapshotType)
            {
                case SnapshotType.Stage:
                    foreach (IndexEntry indexEntry in repo.Index.Where(x => x.Path == relFilePath))
                    {
                        filename = Path.Combine(tempDiffFileDir, "stage_" + Path.GetFileName(relFilePath));
                        Directory.CreateDirectory(tempDiffFileDir);
                        WriteBlobToFile(filename, repo.Lookup(indexEntry.Id) as Blob);
                        break;
                    }
                    break;

                case SnapshotType.WorkingDirectory:
                    filename = Path.Combine(repo.Info.WorkingDirectory, relFilePath);
                    if (!File.Exists(filename))
                    {
                        // The only way this should happen is if it's a deleted file in the working directory
                        filename = Path.Combine(tempDiffFileDir, "wddeleted_" + Path.GetFileName(relFilePath));
                        Directory.CreateDirectory(tempDiffFileDir);
                        File.WriteAllText(filename, "");
                    }
                    break;

                case SnapshotType.Commit:
                    filename = Path.Combine(tempDiffFileDir, this.Commit.Id.ToString(8) + "_" + Path.GetFileName(relFilePath));
                    TreeEntry ti = this.Commit.Tree[relFilePath];
                    Directory.CreateDirectory(tempDiffFileDir);
                    WriteBlobToFile(filename, ti?.Target as Blob);
                    break;

                case SnapshotType.Unknown:
                default:
                    Console.WriteLine("Unhandled case!?");
                    break;
            }

            return filename;
        }

        public static void WriteBlobToFile(string filename, Blob blob)
        {
            if (blob == null)
            {
                File.WriteAllText(filename, "");
            }
            else if (blob.IsBinary)
            {
                using (Stream file = File.Create(filename))
                {
                    blob?.GetContentStream().CopyTo(file);
                }
            }
            else
            {
                Stream binstream = blob?.GetContentStream();

                int byte0 = binstream.ReadByte();
                int byte1 = binstream.ReadByte();
                int byte2 = binstream.ReadByte();
                binstream.Seek(0, SeekOrigin.Begin);
                bool hasBOM = (byte0 == 239 && byte1 == 187 && byte2 == 191);

                string text;
                using (StreamReader sr = new StreamReader(binstream, false))
                {
                    text = sr.ReadToEnd();
                }

                if (text.Contains("\r\n"))
                {
                    // At least one \r\n.. so likely in DOS format.
                }
                else
                {
                    // No \r\n, so likely UNIX format.
                    text = text.Replace("\n", "\r\n");
                }

                if (hasBOM)
                {
                    File.WriteAllText(filename, text, Encoding.UTF8);
                }
                else
                {
                    File.WriteAllText(filename, text);
                }
            }
        }
    }

}
