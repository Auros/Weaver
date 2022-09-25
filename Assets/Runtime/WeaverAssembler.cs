using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LibGit2Sharp;
using Weaver.Models;
using Tree = LibGit2Sharp.Tree;

namespace Weaver
{
    /// <summary>
    /// Builds a "weaver" from a respository
    /// </summary>
    public sealed class WeaverAssembler : IDisposable
    {
        /// <summary>
        /// The snapshots associated with this weaver. This is guaranteed to contain at least one element.
        /// </summary>
        public WeaverSnapshot[] Snapshots { get; private set; }

        private Repository _repo;

        private WeaverAssembler(int size, Repository repo)
        {
            _repo = repo;
            Snapshots = new WeaverSnapshot[size];
        }
        
        public static async UniTask<WeaverAssembler> Create(string filePath, LoadType loadType = LoadType.Synchronous)
        {
            var sw = Stopwatch.StartNew();
            
            Repository repo = new(filePath);

            // Order the commits from beginning to end.
            var commits = repo.Commits.OrderBy(c => c.Committer.When).ToArray();
            
            // Initialize the assembler
            WeaverAssembler assembler = new(commits.Length, repo);
            var startTime = commits[0].Committer.When.ToUnixTimeSeconds();
            var endTime = commits[^1].Committer.When.ToUnixTimeSeconds();

            // Generate every snapshot from each commit
            switch (loadType)
            {
                case LoadType.Synchronous:
                    for (int i = 0; i < assembler.Snapshots.Length; i++)
                        assembler.Snapshots[i] = GenerateSnapshotFromCommit(commits[i], null, false, ref startTime, ref endTime);
                    break;
                case LoadType.Parallel:
                    var result = Parallel.ForEach(Enumerable.Range(0, assembler.Snapshots.Length), i =>
                    {
                        assembler.Snapshots[i] = GenerateSnapshotFromCommit(
                            commits[i],
                            null,
                            false,
                            // ReSharper disable once AccessToModifiedClosure
                            ref startTime,
                            // ReSharper disable once AccessToModifiedClosure
                            ref endTime
                        );
                    });
    
                    while (!result.IsCompleted)
                        await UniTask.NextFrame();
                    break;
                case LoadType.LazyLoaded:
                    for (int i = 0; i < assembler.Snapshots.Length; i++)
                        assembler.Snapshots[i] = GenerateSnapshotFromCommit(commits[i], null, true, ref startTime, ref endTime);
                    break;
                case LoadType.ParallelLazyLoaded:
                    var resultLazyLoaded = Parallel.ForEach(Enumerable.Range(0, assembler.Snapshots.Length), i =>
                    {
                        assembler.Snapshots[i] = GenerateSnapshotFromCommit(
                            commits[i],
                            null,
                            true,
                            // ReSharper disable once AccessToModifiedClosure
                            ref startTime,
                            // ReSharper disable once AccessToModifiedClosure
                            ref endTime
                        );
                    });
                    while (!resultLazyLoaded.IsCompleted)
                        await UniTask.NextFrame();
                    break;
                case LoadType.SynchronousLookBehind:
                    WeaverSnapshot? previous = null;
                    for (int i = 0; i < assembler.Snapshots.Length; i++)
                        previous = assembler.Snapshots[i] = GenerateSnapshotFromCommit(commits[i], previous, false, ref startTime, ref endTime);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(loadType), loadType, null);
            }
            
            sw.Stop();
            var ms = sw.Elapsed.TotalMilliseconds;
            var hi = $"{ms}ms";
            UnityEngine.Debug.Log(hi);

            return assembler;
        }

        private static WeaverSnapshot GenerateSnapshotFromCommit(Commit commit, WeaverSnapshot? previousSnapshot, bool lazyLoad, ref long startTime, ref long endTime)
        {
            // Calculate the normalized snapshot time by performing an inverse lerp.
            var currentTime = commit.Committer.When.ToUnixTimeSeconds();
            var normalizedTime = (currentTime - startTime) * 1.0 / (endTime - startTime);

            // Generate the owner of this snapshot.
            WeaverOwner owner = new(commit.Author.Email, commit.Author.Name);

            Func<WeaverNode> createAncestor = () => CreateNodeFromTree(commit.Tree, string.Empty, owner, 0, previousSnapshot?.Ancestor);

            var time = (float)normalizedTime;
            
            // Generate the snapshot
            var snapshot = lazyLoad
                ? new WeaverSnapshot(time, createAncestor)
                : new WeaverSnapshot(time, createAncestor.Invoke());
            
            return snapshot;
        }

        private static WeaverNode CreateNodeFromTree(Tree tree, string name, WeaverOwner owner, int generation = 0, WeaverNode? previousNode = null)
        {
            if (previousNode != null && previousNode.Hash == tree.Sha)
                return previousNode;
            
            var items = tree
                .Where(t => t.TargetType is TreeEntryTargetType.Blob)
                .Select(entry =>
                {
                    WeaverItem? previousItem = null;
                    
                    if (previousNode == null)
                        return CreateItemFromEntry(entry);
                    
                    for (int i = 0; i < previousNode.Items.Length; i++)
                    {
                        if (previousNode.Items[i].Hash != entry.Target.Sha)
                            continue;
                        previousItem = previousNode.Items[i];
                        break;
                    }

                    return previousItem ?? CreateItemFromEntry(entry);
                    
                })
                .ToArray();
            
            var children = tree
                .Where(t => t.TargetType is TreeEntryTargetType.Tree)
                .Select(entry => CreateNodeFromTree((entry.Target as Tree)!, entry.Path, owner, generation + 1))
                .ToArray();

            WeaverNode node = new(name, tree.Sha, owner, items, children, generation);
            for (int i = 0; i < children.Length; i++)
                children[i].Parent = node;
            
            return node;
        }

        private static WeaverItem CreateItemFromEntry(TreeEntry entry) => new(entry.Target.Sha, entry.Path);

        public void Dispose()
        {
            _repo.Dispose();
        }

        public enum LoadType
        {
            /// <summary>
            /// Loads the repository synchronously on the thread the load method was called on. For smaller repos, this is the fastest.
            /// </summary>
            Synchronous,
            
            /// <summary>
            /// Loads the repository parallel using all available threads. For medium sized repositories, this can potentially be faster.
            /// </summary>
            Parallel,
            
            /// <summary>
            /// Loads the repository synchronously, but doesn't build the node until it is accessed.
            /// </summary>
            LazyLoaded,
            
            /// <summary>
            /// Loads the repository parallel using all available threads to calculate the initial objects,
            /// then will load any subsequent node accesses synchronously when accessed. Best option for large repositories
            /// with light nesting and many commits.
            /// </summary>
            ParallelLazyLoaded,
            
            /// <summary>
            /// Loads the repository synchronously. While building a snapshot, it'll compare to the previous snapshot.
            /// If the node or item is the same, the reference from the previous will get copied to the current.
            /// Uses the least amount of memory. Best option for large repositories.
            /// </summary>
            SynchronousLookBehind
            
        }
    }
}