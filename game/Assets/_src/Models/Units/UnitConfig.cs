using System;
using Common.Defs;

using Unity.Entities;

using UnityEngine;

namespace Game.Model.Units
{
    /// <summary>
    /// Конфиг корабля
    /// </summary>
    [CreateAssetMenu(fileName = "Unit", menuName = "Configs/Unit")]
    public class UnitConfig: ScriptableConfig
    {
        public Unit.UnitConfig Value = new Unit.UnitConfig();
        protected override void Configurate(Entity prefab, IDefineableContext context)
        {
            Value.AddComponentData(prefab, context);
        }
    }
}
