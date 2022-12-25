using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Model.Stats
{
    /*
    /// <summary>
    /// Реализация уровня игровой сущности
    /// Является модификатором (IModifier) для изменения статов
    /// </summary>
    [Serializable]
    public class Level: Entity<LevelConfig>, ILevelWritable, IModifier
    {
        [SerializeField]
        private int m_Value;

        List<IStat> m_Stats = new List<IStat>();

        public override void Create(IEntity parent, LevelConfig config)
        {
            base.Create(parent, config);
            m_Value = Config.Level;
        }

        #region IModifier
        /// <summary>
        /// Множитель на который увеличивается значение статов
        /// </summary>
        public float Multiplier => Mathf.Pow(m_Value, Config.Multiplier);

        /// <summary>
        /// Добавляет модификатор на все статы объекта
        /// </summary>
        public void Attach(IGameEntity entity)
        {
            foreach (var iter in entity.GetStats())
            {
                m_Stats.Add(iter);
                using (var writer = iter.GetWritable())
                {
                    writer.AddMod(this);
                    writer.ResetValue();
                }
            }
        }

        /// <summary>
        /// Убирает модификатор со всех статов объекта
        /// </summary>
        public void Dettach(IGameEntity entity)
        {
            foreach (var iter in entity.GetStats())
            {
                m_Stats.Remove(iter);
                using (var writer = iter.GetWritable())
                {
                    writer.RemoveMod(this);
                    writer.ResetValue();
                }
            }
        }
        #endregion
        #region ILevel
        public int Value => m_Value;

        public ILevelWritable GetWritable()
        {
            return this;
        }

        public void Dispose()
        {
            Update();
        }

        #endregion
        #region ILevelWritable
        public void Inc(int value)
        {
            m_Value += value;
        }
        public void SetValue(int value)
        {
            m_Value = value;
        }
        #endregion

        public void Update()
        {
            foreach (var iter in m_Stats)
            {
                using (var writer = iter?.GetWritable())
                {
                    writer?.ResetValue();
                }
            }
        }
    }
    */
}