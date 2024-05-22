using System;
using Game.Core;
using Game.Model.Logics;

using Reflex.Core;
using Reflex.Attributes;
using Reflex.Injectors;

using Unity.Entities;

namespace Game.Model
{
    public abstract class BaseContext
    {
        [Inject] protected readonly LogicApi m_LogicApi;
        public abstract LogicHandle LogicHandle { get; }
        public void SetWorldState<TWorldState>(Entity entity, TWorldState worldState, bool value)
            where TWorldState : struct, IConvertible => this.SetWorldState(entity, GoalHandle.FromEnum(worldState, value)); 
        public void SetWorldState(Entity entity, GoalHandle value) => m_LogicApi.SetWorldState(entity, value);

        public record DataRecord(Entity Entity, float Delta, StructRef<EntityCommandBuffer.ParallelWriter> Writer, int SortKey);
        public record DataGlobal(Func<Container> GetContainer);
    }

    public abstract class BaseContext<T1, T2> : BaseContext, Logic.ILogicContext
        where T1 : BaseContext.DataRecord
        where T2 : BaseContext.DataGlobal
    {
        protected T1 m_Record;
        protected T2 m_Global;

        public Entity Entity => m_Record.Entity;
        public float Delta => m_Record.Delta;
        public ref EntityCommandBuffer.ParallelWriter Writer => ref m_Record.Writer.Value;
        public int SortKey => m_Record.SortKey;
        
        public void SetWorldState<TWorldState>(Entity entity, TWorldState worldState, bool value)
            where TWorldState : struct, IConvertible => this.SetWorldState(entity, GoalHandle.FromEnum(worldState, value)); 
        public void SetWorldState(Entity entity, GoalHandle value) => m_LogicApi.SetWorldState(entity, value);
        
        
        public class ContextManager<TC>
            where TC : BaseContext<T1, T2>, new()
        {
            private Pool m_Pool;
            private DataGlobal m_Global;
            
            public void Initialization(T2 data)
            {
                m_Global = data;
                m_Pool = new Pool(data);
            }
            
            public TC Get(T1 data)
            {
                var context = m_Pool.Get(data);
                return context;
            }

            public void Release(TC context)
            {
                m_Pool.Release(context);
            }
        
            private class Pool : System.Pool.ObjectPool<TC>
            {
                public Pool(T2 data) : base(
                    arg =>
                    {
                        var context = new TC();
                        context.m_Record = (T1)arg;
                        context.m_Global = data;
                        AttributeInjector.Inject(context, data.GetContainer());
                        return context;
                    },
                    (context, arg) =>
                    {
                        context.m_Record = (T1)arg;
                        context.m_Global = data;
                    })
                {
                    
                }
            } 
        }
    }
}