using System;
using Game.Core;
using Game.Core.Repositories;
using Game.Model.Logics;

using Reflex.Core;
using Reflex.Attributes;
using Reflex.Injectors;

using Unity.Collections;
using Unity.Entities;

namespace Game.Model.Weapons
{
    public partial struct Weapon
    {
        public class Context: BaseContext<Context.ContextRecord, Context.ContextGlobal>, Logic.ILogicContext<Weapon>
        {
            [Inject] public ObjectRepository ObjectRepository { get; private set; }
            
            private static LogicHandle s_LogicHandle = LogicHandle.From<Weapon>();
            public override LogicHandle LogicHandle => s_LogicHandle;
            public Logic.Aspect.Lookup LookupLogic => m_Record.LookupLogic;
            public ref WeaponAspect Weapon => ref m_Record.Weapon.Value;
            public ComponentLookup<Team> LookupTeam => m_Record.LookupTeams;
            
            public record ContextGlobal(Func<Container> GetContainer)
                : DataGlobal(GetContainer);
            public record ContextRecord(StructRef<WeaponAspect> Weapon, float Delta, StructRef<EntityCommandBuffer.ParallelWriter> Writer, int SortKey, Logic.Aspect.Lookup LookupLogic, ComponentLookup<Team> LookupTeams): 
                DataRecord(Weapon.Value.Self, Delta, Writer, SortKey);
        }
    }
}