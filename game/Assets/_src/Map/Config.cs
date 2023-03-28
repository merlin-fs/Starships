using System;
using UnityEngine;
using Unity.Entities;
using Common.Defs;

namespace Game.Model.Worlds
{
    using Core.Defs;

    [CreateAssetMenu(fileName = "Map", menuName = "Configs/Map")]
    public class WeaponConfig : GameObjectConfig
    {
        public Map.Data.Def Value = new Map.Data.Def();

        protected override void Configurate(Entity prefab, IDefineableContext context)
        {
            base.Configurate(prefab, context);
            Value.AddComponentData(prefab, context);
        }
    }
}
