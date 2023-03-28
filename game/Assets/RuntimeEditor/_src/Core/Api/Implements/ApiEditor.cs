using System;
using System.Threading.Tasks;
using Unity.Entities;
using Common.Defs;
using Game;
using Game.Core;
using Game.Core.Prefabs;
using Game.Core.Events;

namespace Buildings
{
    using Environments;

    public class ApiEditor: IApiEditor, IKernel
    {
        public IEventHandler Events { get; }

        private EventDispatcher m_Dispatcher = EventDispatcher.CreateDefault();

        public ApiEditor()
        {
            Events = new ProxyEvents(this);
        }

        void IKernel.SendEvent(EventBase e)
        {
            m_Dispatcher.Dispatch(e, this, DispatchMode.Default);
        }

        void IKernel.InvokeCallbacks(EventBase evt, PropagationPhase propagationPhase)
        {
            (Events as ProxyEvents).InvokeCallbacks(evt, propagationPhase);
        }

        public async Task<IPlaceHolder> AddEnvironment(IConfig config)
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var prefabs = manager.World.GetOrCreateSystemManaged<PrefabEnvironmentSystem>();
            await prefabs.IsDone();

            var ecb = manager.World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>()
                .CreateCommandBuffer();
            var item = ecb.Instantiate(config.Prefab);
            var holder = new Placeholder(this);
            ecb.AddComponent<SelectBuildingTag>(item);

            return holder;
        }

        private struct Placeholder : IPlaceHolder
        {
            private ApiEditor m_Kernel;
            
            IEventHandler IPlaceHolder.Events => m_Kernel.Events;

            public Placeholder(ApiEditor kernel)
            {
                m_Kernel = kernel;
            }
        }
    }
}