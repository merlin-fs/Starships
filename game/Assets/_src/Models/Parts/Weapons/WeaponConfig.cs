using System;
using UnityEngine;

namespace Game.Model.Weapons
{
    using Logics;
    using Result = Logics.Logic.Result;

    /// <summary>
    /// Конфиг оружия
    /// </summary>
    [CreateAssetMenu(fileName = "Weapon", menuName = "Configs/Parts/Weapon")]
    public class WeaponConfig: ScriptableObject, IInitializable
    {
        public Weapon.WeaponConfig Value = new Weapon.WeaponConfig();

        public Logic.Config Logic = new Logic.Config();
        public void Init()
        {
            if (Application.isPlaying)
                Logic.Configure()
                    .Transition(Result.Done, null, Weapon.State.Reload)

                    .Transition(Result.Done, Weapon.State.Reload, Weapon.State.Shooting)
                    .Transition(Result.Error, Weapon.State.Reload, Weapon.State.Sleep)

                    .Transition(Result.Done, Weapon.State.Shooting, Weapon.State.Reload);
        }
    }
}
