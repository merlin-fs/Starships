using System;
using Unity.Entities;
using UnityEngine;
using Common.Defs;

namespace Game.Model
{
    using Core.Defs;

    public enum DamageTargets
    {
        One,
        AoE,
    }

    /// <summary>
    /// Тип урона
    /// </summary>
    [CreateAssetMenu(fileName = "DamageType", menuName = "Configs/DamageType")]
    public class DamageType : GameObjectConfig
    {
    }
}