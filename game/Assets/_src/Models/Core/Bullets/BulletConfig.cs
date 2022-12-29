using System;
using Common.Defs;

using UnityEngine;

namespace Game.Model.Weapons
{
    [CreateAssetMenu(fileName = "Bullet", menuName = "Configs/Bullet")]
    public class BulletConfig: ScriptableConfig
    {
        public Bullet.Config Value;
    }
}
