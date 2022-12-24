using System;
using Game.Model.Weapons;

using UnityEngine;

namespace Game.Model
{
    [CreateAssetMenu(fileName = "Logic", menuName = "Configs/Logic")]
    public class LogicConfig : ScriptableObject
    {
        public Logic.Config Value = new Logic.Config();

        public void Init()
        {
            if (Application.isPlaying)
                Value.Configure()
                    .Transition<Weapon.Reload>(null, Weapon.States.Reload)
                    .Transition<Weapon.Reload>(Weapon.States.Reload, null);
        }
    }
}
