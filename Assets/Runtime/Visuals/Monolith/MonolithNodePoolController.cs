using VContainer;

namespace Weaver.Visuals.Monolith
{
    public class MonolithNodePoolController : InjectablePoolController<MonolithNode>
    {
        [Inject]
        private new readonly IObjectResolver _container = null!;

        protected override void Start()
        {
            base._container = _container;
            base.Start();
        }

        // Unlink the node coming into the object pool from it's parent.
        protected override void ReleaseObject(MonolithNode node)
        {
            node.UnlinkParent();
            base.ReleaseObject(node);
        }
    }
}