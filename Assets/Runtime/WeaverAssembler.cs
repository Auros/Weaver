using System.Linq;
using LibGit2Sharp;
using Weaver.Models;
using Tree = LibGit2Sharp.Tree;

namespace Weaver
{
    /// <summary>
    /// Builds a "weaver" from a respository
    /// </summary>
    public sealed class WeaverAssembler
    {
        /// <summary>
        /// The snapshots associated with this weaver. This is guaranteed to contain at least one element.
        /// </summary>
        public WeaverSnapshot[] Snapshots { get; }
        
        public WeaverAssembler(string filePath)
        {
            using Repository repo = new(filePath);

            // Order the commits from beginning to end.
            var commits = repo.Commits.OrderBy(c => c.Committer.When).ToArray();
            
            // Fill our snapshot array
            Snapshots = new WeaverSnapshot[commits.Length];

            var startTime = commits[0].Committer.When.ToUnixTimeSeconds();
            var endTime = commits[^1].Committer.When.ToUnixTimeSeconds();
            
            // Generate every snapshot from each commit
            for (int i = 0; i < Snapshots.Length; i++)
                Snapshots[i] = GenerateSnapshotFromCommit(commits[i], ref startTime, ref endTime);
        }

        private static WeaverSnapshot GenerateSnapshotFromCommit(Commit commit, ref long startTime, ref long endTime)
        {
            // Calculate the normalized snapshot time by performing an inverse lerp.
            var currentTime = commit.Committer.When.ToUnixTimeSeconds();
            var normalizedTime = (currentTime - startTime) * 1.0 / (endTime - startTime);

            // Generate the snapshot
            var ancestor = CreateNodeFromTree(commit.Tree, string.Empty);
            WeaverSnapshot snapshot = new((float)normalizedTime, ancestor);

            return snapshot;
        }

        private static WeaverNode CreateNodeFromTree(Tree tree, string name)
        {
            var items = tree
                .Where(t => t.TargetType is TreeEntryTargetType.Blob)
                .Select(CreateItemFromEntry)
                .ToArray();
            
            var children = tree
                .Where(t => t.TargetType is TreeEntryTargetType.Tree)
                .Select(entry => CreateNodeFromTree((entry.Target as Tree)!, entry.Path))
                .ToArray();

            WeaverNode node = new(name, items, children);
            for (int i = 0; i < children.Length; i++)
                children[i].Parent = node;
            
            return node;
        }

        private static WeaverItem CreateItemFromEntry(TreeEntry entry) => new(entry.Target.Sha, entry.Path);
    }
}