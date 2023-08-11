using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Common.Defs;

namespace Game.Model.Weapons
{
    using Stats;
    using Logics;
    using Core.Defs;

    /// <summary>
    /// Конфиг оружия
    /// </summary>
    [CreateAssetMenu(fileName = "Weapon", menuName = "Configs/Parts/Weapon")]
    public class WeaponConfig: GameObjectConfig, IConfigStats, IConfigContainer
    {
        public List<ChildConfig> Parts = new List<ChildConfig>();
        public Weapon.WeaponDef Value = new Weapon.WeaponDef();
        public LogicConfig Logic;

        protected override void Configurate(Entity prefab, IDefineableContext context)
        {
            base.Configurate(prefab, context);
            Value.AddComponentData(prefab, context);
            if (Logic is IConfig config)
                config.Configurate(prefab, context);
        }

        IEnumerable<ChildConfig> IConfigContainer.Childs => Parts;

        void IConfigStats.Configurate(DynamicBuffer<Stat> stats)
        {
            stats.AddStat(Global.Stats.Health, Value.Health);
            stats.AddStat(Weapon.Stats.Rate, Value.Rate);
            stats.AddStat(Weapon.Stats.Damage, Value.DamageValue);
            stats.AddStat(Weapon.Stats.ReloadTime, Value.ReloadTime);
            stats.AddStat(Weapon.Stats.ClipSize, Value.ClipSize);
            stats.AddStat(Weapon.Stats.Range, Value.Range);
        }
    }
}
