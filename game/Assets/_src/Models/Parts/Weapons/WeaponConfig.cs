using System;
using UnityEngine;
using Common.Defs;
using Unity.Entities;

namespace Game.Model.Weapons
{
    using Logics;

    /// <summary>
    /// Конфиг оружия
    /// </summary>
    [CreateAssetMenu(fileName = "Weapon", menuName = "Configs/Parts/Weapon")]
    public class WeaponConfig: ScriptableConfig, IInitializable
    {
        public Weapon.WeaponConfig Value = new Weapon.WeaponConfig();
        public Logic.Config Logic = new Logic.Config();

        protected override void Configurate(Entity prefab, IDefineableContext context)
        {
            Value.AddComponentData(prefab, context);
            Logic.AddComponentData(prefab, context);
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
