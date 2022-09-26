namespace Weaver.Models
{
    /// <summary>
    /// The owner acts like a committer within weaver, when a node is created, it has an "owner" who essentially
    /// acts as the reason why that node was created.
    /// </summary>
    public sealed class WeaverOwner
    {
        /// <summary>
        /// The identifier of this owner, you can expect this to be the email, as long as it's unique.
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// The display name of this owner.
        /// </summary>
        public string Name { get; }

        public WeaverOwner(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}