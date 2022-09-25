using System;
using LibGit2Sharp;

namespace Weaver.Models
{
    public sealed class WeaverSnapshot
    {
        /// <summary>
        /// The normalized (0.0-1.0) time this snapshot was taken.
        /// </summary>
        public float Time { get; }

        /// <summary>
        /// Every snapshot must have a parent node.
        /// </summary>
        public WeaverNode Ancestor => _ancestor ??= _ancestorBuilder!.Invoke();

        private WeaverNode? _ancestor;
        private readonly Func<WeaverNode>? _ancestorBuilder;
        
        public Tree? Previous { get; set; }
        public Tree? Next { get; set; }
        
        public WeaverSnapshot(float time, WeaverNode ancestor)
        {
            Time = time;
            _ancestor = ancestor;
        }

        public WeaverSnapshot(float time, Func<WeaverNode> ancestorBuilder)
        {
            Time = time;
            _ancestorBuilder = ancestorBuilder;
        }
    }
}