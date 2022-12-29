using System;
using Common.Defs;
using Unity.Entities;
using UnityEngine;

namespace Game.Model
{
    /// <summary>
    /// Тип урона
    /// </summary>
    [CreateAssetMenu(fileName = "DamageType", menuName = "Configs/DamageType")]
    public class DamageType : ScriptableConfig
    {
        protected override void Configurate(Entity prefab, IDefineableContext context)
        {
        }
    }
}