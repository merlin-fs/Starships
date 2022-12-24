using System;
using Game.Model.Weapons;

using UnityEngine;

namespace Game.Model
{
    using Result = ILogic.Result;


    [CreateAssetMenu(fileName = "Logic", menuName = "Configs/Logic")]
    public class LogicConfig : ScriptableObject
    {
        public Logic.Config Value = new Logic.Config();

        public void Init()
        {
            if (Application.isPlaying)
                Value.Configure()
                    .Transition(Result.Done, null, Weapon.State.Reload)

                    .Transition(Result.Done, Weapon.State.Reload, Weapon.State.Shooting)
                    .Transition(Result.Error, Weapon.State.Reload, Weapon.State.Sleep)

                    .Transition(Result.Done, Weapon.State.Shooting, Weapon.State.Reload);
        }
    }
}
