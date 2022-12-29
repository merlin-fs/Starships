using System;
using Common.Defs;
using Unity.Entities;
using UnityEngine;

namespace Game.Model.Weapons
{
    [CreateAssetMenu(fileName = "Bullet", menuName = "Configs/Bullet")]
    public class BulletConfig: ScriptableConfig
    {
        public Bullet.Config Value;
        protected override void Configurate(Entity prefab, IDefineableContext context)
        {
        }
    }
}
