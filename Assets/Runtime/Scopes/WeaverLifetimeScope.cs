using System;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Weaver.Models;

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


            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<WeaverAssembler?>(options);

        }
    }
}