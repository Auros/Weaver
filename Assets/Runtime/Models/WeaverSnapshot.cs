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
        public WeaverNode Ancestor { get; }

        public WeaverSnapshot(float time, WeaverNode ancestor)
        {
            Time = time;
            Ancestor = ancestor;
        }
    }
}