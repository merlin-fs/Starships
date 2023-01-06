using System;
using Unity.Entities;
using UnityEngine;
using Common.Defs;

namespace Game.Model.Weapons
{
    using Core.Defs;

    [CreateAssetMenu(fileName = "Bullet", menuName = "Configs/Bullet")]
    public class BulletConfig: GameObjectConfig
    {
        public Bullet.BulletDef Value;
        protected override void Configurate(Entity prefab, IDefineableContext context)
        {
        }
    }
}
