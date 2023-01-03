using System;
using Common.Defs;
using Unity.Entities;
using UnityEngine;

namespace Game.Model.Units
{
    using Game.Model.Stats;
    using Logics;

    using static UnityEngine.EventSystems.EventTrigger;

    /// <summary>
    /// Конфиг корабля
    /// </summary>
    [CreateAssetMenu(fileName = "Unit", menuName = "Configs/Unit")]
    public class UnitConfig: ScriptableConfig
    {
        public Unit.UnitConfig Value = new Unit.UnitConfig();
        public Logic.Config Logic = new Logic.Config();
        public Team.Def Team = new Team.Def();

        protected override void Configurate(Entity prefab, IDefineableContext context)
        {
            Value.AddComponentData(prefab, context);
            Team.AddComponentData(prefab, context);
            Logic?.AddComponentData(prefab, context);
        }

        public override void OnAfterDeserialize()
        {
            Init();
        }

        public void Init()
        {
            Logic.Init();
        }
    }
}
