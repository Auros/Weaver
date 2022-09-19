using JetBrains.Annotations;

namespace Weaver.Models
{
    /// <summary>
    /// A Weaver Node represents a collection of items within a Weaver Graph. It can be treated like a "directory".
    /// The node should be immutable.
    /// </summary>
    [PublicAPI]
    public sealed class WeaverNode
    {
        /// <summary>
        /// The items in this node. These can be treated like the "files" in this "directory".
        /// </summary>
        public WeaverItem[] Items { get; }

        /// <summary>
        /// The parent node.
        /// </summary>
        public WeaverNode? Parent { get; set; }
        
        /// <summary>
        /// The child nodes belonging to this node.
        /// </summary>
        public WeaverNode[] Children { get; }
        
        public WeaverNode(WeaverItem[] items, WeaverNode[] children)
        {
            Items = items;
            Children = children;
        }
    }
}