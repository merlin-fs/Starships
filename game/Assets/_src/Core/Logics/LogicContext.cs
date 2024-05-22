using System;
using Game.Core;

using Reflex.Core;
using Reflex.Attributes;
using Reflex.Injectors;

using Unity.Entities;

namespace Game.Model.Logics
{
    public partial struct Logic 
    {
        public class Context: ILogicContext<Logic>
        {
            private static Pool m_Pool;

            [Inject] private readonly LogicApi m_Api;
            
            public LogicHandle LogicHandle => LogicHandle.From<Logic>();
            public Entity Entity { get; private set; }
            public float Delta { get; private set; }
            public RefRW<Logic> Logic { get; private set; }

            public void SetWorldState(Entity entity, GoalHandle value)
            {
                m_Api.SetWorldState(entity, value);
            }
            
            public static void Initialization(Container container)
            {
                m_Pool = new Pool(container);
            }
            
            public static Context Get(Entity entity, RefRW<Logic> logic, float delta)
            {
                var context = m_Pool.Get();
                context.Entity = entity;
                context.Logic = logic;
                context.Delta = delta;
                return context;
            }

            public static void Release(Context context)
            {
                m_Pool.Release(context);
            }

            private class Pool : UnityEngine.Pool.ObjectPool<Context>
            {
                public Pool(Container container) : base(
                    () =>
                    {
                        var context = new Context();
                        AttributeInjector.Inject(context, container);
                        return context;
                    },
                    context => { },
                    context => { },
                    context => { },
                    true)
                {
                    
                }
            } 
        }
        
    }
}