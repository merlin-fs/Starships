using Common.Defs;
using System;
using UnityEngine;

namespace Game.Model
{
    [CreateAssetMenu(fileName = "Bullet", menuName = "Configs/Bullet")]
    public class BulletConfig: ScriptableObject
    {
        public Bullet.Config Value;
    }
}
