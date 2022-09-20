using JetBrains.Annotations;

namespace Weaver.Models
{
    /// <summary>
    /// A Weaver Item represents an item within a Weaver Node.It can be treated like a "file".
    /// The item should be immutable.
    /// </summary>
    [PublicAPI]
    public sealed class WeaverItem
    {
        /// <summary>
        /// The hash of this item. If this item "changes", this property will have a
        /// different value compared to another item, but the name will be the same.
        /// </summary>
        public string Hash { get; }
        
        /// <summary>
        /// The name of this item. Used for display purposes.
        /// </summary>
        public string Name { get; }
        
        public WeaverItem(string hash, string name)
        {
            Hash = hash;
            Name = name;
        }
    }
}