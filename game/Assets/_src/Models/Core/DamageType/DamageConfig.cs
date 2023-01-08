using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Model
{
    using Core.Defs;

    /// <summary>
    /// Тип урона
    /// </summary>
    [CreateAssetMenu(fileName = "DamageType", menuName = "Configs/DamageType")]
    public class DamageConfig : GameObjectConfig
    {
        [SerializeReference, ReferenceSelect(typeof(IDamage))]
        public List<IDamage> Damages = new List<IDamage>();
    }
}