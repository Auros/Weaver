namespace Weaver.Models
{
    public sealed class WeaverItemEvent
    {
        public WeaverItem Item { get; set; } = null!;
        public WeaverNode Node { get; set; } = null!;
    }
}