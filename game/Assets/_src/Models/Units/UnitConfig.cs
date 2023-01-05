using System;
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
    public class UnitConfig: GameObjectConfig, IConfigStats
    {
        public Unit.UnitDef Value = new Unit.UnitDef();
        public Logic.Config Logic = new Logic.Config();
        public Team.Def Team = new Team.Def();

        protected override void Configurate(Entity prefab, IDefineableContext context)
        {
            base.Configurate(prefab, context);
            Value.AddComponentData(prefab, context);
            Team.AddComponentData(prefab, context);
            Logic?.AddComponentData(prefab, context);
        }

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
