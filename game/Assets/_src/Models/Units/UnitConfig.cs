using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Common.Defs;

namespace Game.Model.Units
{
    using Stats;
    using Logics;
    using Core.Defs;

    /// <summary>
    /// Конфиг корабля
    /// </summary>
    [CreateAssetMenu(fileName = "Unit", menuName = "Configs/Unit")]
    public class UnitConfig: GameObjectConfig, IConfigContainer, IConfigStats
    {
        public Unit.UnitDef Value = new Unit.UnitDef();
        public LogicConfig Logic;
        public Team.Def Team = new Team.Def();

        protected override void Configure(Entity prefab, IDefinableContext context)
        {
            base.Configure(prefab, context);
            context.AddComponentData(prefab, new Root());
            Value.AddComponentData(prefab, context);
            Team.AddComponentData(prefab, context);
            
            if (Logic is IConfig config)
                config.Configure(prefab, context);
        }

        IEnumerable<ChildConfig> IConfigContainer.Childs => Value.Parts;

        void IConfigStats.Configure(DynamicBuffer<Stat> stats)
        {
            stats.AddStat(Global.Stats.Health, Value.Health.Value);
            stats.AddStat(Unit.Stats.Speed, Value.Speed.Value);
        }
    }
}
