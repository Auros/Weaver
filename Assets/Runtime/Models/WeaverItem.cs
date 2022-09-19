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
        /// The name of this item. Used for display purposes, but can also be used as a unique ID
        /// </summary>
        public string Name { get; }

        public WeaverItem(string name)
        {
            Name = name;
        }
    }
}