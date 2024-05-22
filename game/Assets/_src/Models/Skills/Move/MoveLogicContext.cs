using System;
using Game.Core;
using Game.Model.Logics;

using Reflex.Core;
using Reflex.Attributes;

using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Game.Model
{
    public partial struct Move
    {
        public class Context: BaseContext<Context.ContextRecord, Context.ContextGlobal>, Logic.ILogicContext<Move> 
        {
            private static LogicHandle s_LogicHandle = LogicHandle.From<Move>();
            public override LogicHandle LogicHandle => s_LogicHandle;
            public WorldTransform LookupWorldTransform => m_Record.LookupWorldTransform;
            public Move Move => m_Record.Move;
            public record ContextRecord(Entity Entity, float Delta, StructRef<EntityCommandBuffer.ParallelWriter> Writer,
                    int SortKey, Move Move, WorldTransform LookupWorldTransform)
                : DataRecord(Entity, Delta, Writer, SortKey);
            public record ContextGlobal(Func<Container> GetContainer)
                : DataGlobal(GetContainer);
        }
    }
}