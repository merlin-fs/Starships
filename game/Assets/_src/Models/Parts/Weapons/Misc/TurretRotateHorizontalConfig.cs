using System;
using UnityEngine;
using Unity.Entities;
using Common.Defs;

namespace Game.Model.Weapons
{
    using Core.Defs;

    [CreateAssetMenu(fileName = "TurretRotateHorizontal", menuName = "Configs/Parts/Weapon/TurretRotateHorizontal")]
    public class TurretRotateHorizontalConfig : GameObjectConfig
    {
        public TurretRotateHorizontal.TurretRotateHorizontalDef Value = new TurretRotateHorizontal.TurretRotateHorizontalDef();
        protected override void Configure(Entity prefab, IDefinableContext context)
        {
            base.Configure(prefab, context);
            Value.AddComponentData(prefab, context);
        }
    }
}
