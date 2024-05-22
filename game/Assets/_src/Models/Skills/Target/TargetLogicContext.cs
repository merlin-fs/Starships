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
    public partial struct Target
    {
        public class Context: BaseContext<Context.ContextRecord, Context.ContextGlobal>, Logic.ILogicContext<Target> 
        {
            private static LogicHandle s_LogicHandle = LogicHandle.From<Target>();
            public override LogicHandle LogicHandle => s_LogicHandle;
            public ComponentLookup<LocalToWorld> LookupLocalToWorld => m_Record.LookupLocalToWorld;
            public ComponentLookup<Team> LookupTeams => m_Record.LookupTeam;
            public NativeList<Entity> Entities => m_Record.Entities;
            public Query Query => m_Record.Query;
            public record ContextRecord(Entity Entity, float Delta, StructRef<EntityCommandBuffer.ParallelWriter> Writer,
                    int SortKey, Query Query, NativeList<Entity> Entities, 
                    ComponentLookup<LocalToWorld> LookupLocalToWorld, ComponentLookup<Team> LookupTeam)
                : DataRecord(Entity, Delta, Writer, SortKey);
            public record ContextGlobal(Func<Container> GetContainer)
                : DataGlobal(GetContainer);
        }
    }
}