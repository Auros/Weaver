using MessagePipe;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;
using Weaver.Models;
using Weaver.Visuals.Monolith;

namespace Weaver.Scopes
{
    public class WeaverLifetimeScope : LifetimeScope
    {
        [SerializeField]
        private MonoTimeController _monoTimeController = null!;

        [SerializeField]
        private MonolithItemPoolController _itemPoolController = null!;

        [SerializeField]
        private MonolithNodePoolController _nodePoolController = null!;

        [SerializeField]
        private MonolithOwnerPoolController _ownerPoolController = null!;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<WeaverStateDaemon>();
            builder.RegisterComponent<IClock>(_monoTimeController);

            builder.RegisterComponent(_itemPoolController);
            builder.RegisterComponent(_nodePoolController);
            builder.RegisterComponent(_ownerPoolController);
            
            builder.Register<IObjectPool<MonolithAction>>(
                _ =>new ObjectPool<MonolithAction>(
                    () => new MonolithAction(),
                    actionOnRelease: action => action.Reset()
                    ),
                Lifetime.Singleton);

            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<WeaverAssembler?>(options);
            builder.RegisterMessageBroker<string, WeaverNode>(options);
            builder.RegisterMessageBroker<string, WeaverItemEvent>(options);
        }
    }
}