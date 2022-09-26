using Weaver.Models;

namespace Weaver.Visuals.Monolith
{
    /// <summary>
    /// Monolith Actions are used to store information on node items.
    /// Instead of using nullables, we just use an IsValid method.
    /// </summary>
    public sealed class MonolithAction
    {
        public MonolithActionType Type { get; set; } = MonolithActionType.None;
        
        public WeaverItem Item { get; set; } = null!;
        
        public MonolithNode PhysicalNode { get; set; } = null!;
        
        public bool IsValid() => PhysicalNode != null && Item != null! && Type != MonolithActionType.None;

        public void Reset()
        {
            Type = MonolithActionType.None;
            PhysicalNode = null!;
            Item = null!;
        }
    }
}