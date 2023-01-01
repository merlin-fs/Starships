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
            Logic.Configure()
                .Transition(null, null, Target.State.Find)

                .Transition(Target.State.Find, Target.Result.Found, Weapon.State.Shooting)

                .Transition(Target.State.Find, Target.Result.NoTarget, Weapon.State.Sleep)

                .Transition(Weapon.State.Shooting, Weapon.Result.Done, Weapon.State.Shoot)
                .Transition(Weapon.State.Shooting, Weapon.Result.NoAmmo, Weapon.State.Reload)

                .Transition(Weapon.State.Shoot, Weapon.Result.Done, Target.State.Find)

                .Transition(Weapon.State.Reload, Weapon.Result.Done, Target.State.Find)
                .Transition(Weapon.State.Reload, Weapon.Result.NoAmmo, Weapon.State.Sleep)

                .Transition(Weapon.State.Sleep, Weapon.Result.NoAmmo, Weapon.State.Reload)
                .Transition(Weapon.State.Sleep, Target.Result.NoTarget, Weapon.State.Reload);
        }
    }
}
