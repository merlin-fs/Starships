using System;
using Unity.Entities;
using Common.Defs;
using Common.Core;

namespace Game.Model.Weapons
{
    /// <summary>
    /// Реализация оружия
    /// </summary>
    [Serializable]
    public struct Weapon: IPart, IDefineable, IComponentData, IDefineableCallback
    {
        private readonly Def<WeaponDef> m_Def;
        public WeaponDef Def => m_Def.Value;

        public int Count;

        public float Time;

        public ObjectID BulletID;

        public Weapon(Def<WeaponDef> config)
        {
            m_Def = config;
            Time = 0;
            Count = 0;
            BulletID = m_Def.Value.Bullet.ID;
        }
        #region IDefineableCallback
        public void AddComponentData(Entity entity, IDefineableContext context)
        {
            //context.SetName(entity, GetType().Name);
            context.AddComponentData(entity, new Target());
            Count = Def.ClipSize;
        }
        public void RemoveComponentData(Entity entity, IDefineableContext context) { }
        #endregion

        /// <summary>
        /// Состояние оружия
        /// </summary>
        public enum State
        {
            Init,
            Shooting,
            Shoot,
            Reload,
            Sleep,
        }
        public enum Result
        { 
            Done,
            NoAmmo,
        }

        /// <summary>
        /// Список статов оружия
        /// </summary>
        public enum Stats
        {
            /// <summary>
            /// Урон
            /// </summary>
            Damage,
            /// <summary>
            /// Размер обоймы
            /// </summary>
            ClipSize,
            /// <summary>
            /// Период стрельбы
            /// </summary>
            Rate,
            /// <summary>
            /// Время перезарядки
            /// </summary>
            ReloadTime,
        }

        [Serializable]
        public class WeaponDef : IDef<Weapon>
        {
            public BulletConfig Bullet;
            public int Health = 5;
            public float DamageValue = 1;
            public int BarrelCount;
            public int ClipSize;
            public float Rate = 1;
            public float ReloadTime = 5;
        }
    }
}
