using System;
using UnityEngine;
using Common.Defs;
using Unity.Entities;

namespace Game.Model.Weapons
{
    using Logics;
    using Result = Logics.Logic.Result;

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
                .Transition(Result.Done, null, Target.State.Find)

                .Transition(Result.Done, Target.State.Find, Weapon.State.Shooting)
                .Transition(Result.Error, Target.State.Find, Weapon.State.Sleep)

                .Transition(Result.Done, Weapon.State.Shooting, Target.State.Find)
                .Transition(Result.Error, Weapon.State.Shooting, Weapon.State.Reload)

                .Transition(Result.Done, Weapon.State.Reload, Target.State.Find)
                .Transition(Result.Error, Weapon.State.Reload, Weapon.State.Sleep)

                .Transition(Result.Done, Weapon.State.Sleep, Target.State.Find);
        }
    }
}
