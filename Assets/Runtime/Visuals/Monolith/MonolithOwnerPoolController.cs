using VContainer;

namespace Weaver.Visuals.Monolith
{
    public sealed class MonolithOwnerPoolController : InjectablePoolController<MonolithOwner>
    {
        [Inject]
        private new readonly IObjectResolver _container = null!;
        
        protected override void Start()
        {
            base._container = _container;
            base.Start();
        }
    }
}