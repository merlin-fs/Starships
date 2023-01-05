using System;
using Unity.Entities;
using UnityEngine;
using Common.Defs;

namespace Game.Model
{
    using Core.Defs;

    /// <summary>
    /// Тип урона
    /// </summary>
    [CreateAssetMenu(fileName = "DamageType", menuName = "Configs/DamageType")]
    public class DamageType : GameObjectConfig
    {
        protected override void Configurate(Entity prefab, IDefineableContext context)
        {
        }
    }
}