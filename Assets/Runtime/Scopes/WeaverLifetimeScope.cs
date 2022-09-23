using System;
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
        
        protected override void Configure(IContainerBuilder builder)
        {
            if (!_monoTimeController)
                throw new InvalidOperationException($"The field {nameof(_monoTimeController)} has not been set in the inspector.");
                
            builder.RegisterEntryPoint<WeaverStateDaemon>();
            builder.RegisterComponent<IClock>(_monoTimeController);
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