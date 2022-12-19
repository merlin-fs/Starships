using System;
using UnityEngine;

namespace Game.Model.Weapons
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Configs/Parts/Weapon")]
    /// <summary>
    /// Конфиг оружия
    /// </summary>
    public class WeaponConfig: ScriptableObject
    {
        public Weapon.WeaponConfig Value;
    }
}
