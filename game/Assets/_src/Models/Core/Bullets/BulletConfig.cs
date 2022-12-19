using System;
using UnityEngine;

namespace Game.Model.Weapons
{
    [CreateAssetMenu(fileName = "Bullet", menuName = "Configs/Bullet")]
    public class BulletConfig: ScriptableObject
    {
        public Bullet.Config Value;
    }
}
