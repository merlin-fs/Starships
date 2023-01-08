using System;
using Unity.Entities;
using UnityEngine;
using Common.Defs;

namespace Game.Model.Units
{
    using Stats;
    using Logics;
    using Core.Defs;
    using System.Collections.Generic;

    /// <summary>
    /// Конфиг корабля
    /// </summary>
    [CreateAssetMenu(fileName = "Unit", menuName = "Configs/Unit")]
    public class UnitConfig: GameObjectConfig, IConfigContainer, IConfigStats
    {
        public Unit.UnitDef Value = new Unit.UnitDef();
        public Logic.LogicDef Logic = new Logic.LogicDef();
        public Team.Def Team = new Team.Def();

        protected override void Configurate(Entity prefab, IDefineableContext context)
        {
            base.Configurate(prefab, context);
            Value.AddComponentData(prefab, context);
            Team.AddComponentData(prefab, context);
            if (Logic.IsValid)
                Logic.AddComponentData(prefab, context);
        }

        IEnumerable<ChildConfig> IConfigContainer.Childs => Value.Parts;

        void IConfigStats.Configurate(DynamicBuffer<Stat> stats)
        {
            stats.AddStat(GlobalStat.Health, Value.Health.Value);
            stats.AddStat(Unit.Stats.Speed, Value.Speed.Value);
        }

        public override void OnAfterDeserialize()
        {
            Logic.Init();
        }
    }
}
