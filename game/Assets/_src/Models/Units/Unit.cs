using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Model.Units
{
/*
    using Stats;

    /// <summary>
    /// Реализация юнита (корабля)
    /// </summary>
    [Serializable]
    public class Unit: Entity<UnitConfig>, IUnit
    {
        /// <summary>
        /// Список статов корабля
        /// </summary>
        public enum StatType
        {
            /// <summary>
            /// Скорость
            /// </summary>
            Speed,
        }

        /// <summary>
        /// Уровень корабля
        /// </summary>
        [SerializeReference]
        private ILevel m_Level;

        /// <summary>
        /// Статы для просмотра в Unity.Inspector
        /// 
        /// В идеале нужно или написать свой PropertyDrawer или можно использовать OdinInspector для просмотра свойств
        /// </summary>
        [SerializeReference]
        private List<IStat> m_DebugStats = new List<IStat>();

        /// <summary>
        /// Список креплений корабля
        /// </summary>
        [SerializeField]
        private List<Binding> m_Bindings = new List<Binding>();

        /// <summary>
        /// Статы игровой щусности
        /// 
        /// для сохранения объекта нужно использовать SerializeDictionary либо свой сериализатор
        /// </summary>

        private Dictionary<Enum, IStat> m_Stats = new Dictionary<Enum, IStat>();

        /// <summary>
        /// Создает объект корабля и устанавливает его статы
        /// </summary>
        public override void Create(IEntity parent, UnitConfig config)
        {
            base.Create(parent, config);
            //Создаем свойство Level по его конфинурации
            m_Level = Config.Level.Create<ILevel>(this);
            //Создаем стат HP и подписываемся на изменение
            var stat = new Health(Config.Health);
            stat.OnChange += OnChangeHealth;
            //Добавляем статы
            AddStat(stat);
            AddStat(new Stat(StatType.Speed, Config.Speed));
            
            //Создаем крепления по крфигурации и устрйства в них (если есть)
            foreach (var iter in Config.Bindings)
            {
                var part = iter.Create(this);
                m_Bindings.Add(part);
            }
        }

        /// <summary>
        /// Инициализирует объект и обновляет статы
        /// </summary>
        public override void Init()
        {
            m_Level.Attach(this);
            UpdateStats();
        }

        /// <summary>
        /// Добавляет стат
        /// </summary>
        protected void AddStat(IStat stat)
        {
            if (!m_Stats.ContainsKey(stat.StatType))
            {
                m_Stats.Add(stat.StatType, stat);
                m_DebugStats.Add(stat);
            }
        }

        /// <summary>
        /// Обновляет все статы
        /// </summary>
        protected void UpdateStats()
        {
            foreach (var iter in m_DebugStats)
            {
                iter.Сalculation();
            }
        }

        /// <summary>
        /// Событие изменения HP
        /// </summary>
        /// <param name="stat"></param>
        private void OnChangeHealth(IStat stat)
        {
            //Если HP <= 0 уничтожаем обїект
            if (stat.Value <= 0)
                Destroy();
        }
        #region IUnit
        /// <summary>
        /// Событие уничтожение корабля
        /// </summary>
        public event Action<IGameEntity> OnDestroy;

        /// <summary>
        /// Уровень корабля
        /// </summary>
        public ILevel Level => m_Level;

        /// <summary>
        /// Возвращает список всех статов
        /// </summary>
        public IEnumerable<IStat> GetStats()
        {
            return m_Stats.Values;
        }

        /// <summary>
        /// Возвращает стат по его типу
        /// </summary>
        public IStat GetStat(Enum statType) 
        {
            return m_Stats.TryGetValue(statType, out IStat value) 
                ? value 
                : null;
        }

        /// <summary>
        /// Поиск указаного типа во всех установленных устройствах
        /// </summary>
        public IEnumerable<T> FindParts<T>()
        {
            var parts = Bindings
                .Select(b => b.Part)
                .Union(Bindings
                        .Select(b => b.Part)
                        .SelectMany(c => (c as IComposite) ?? Enumerable.Empty<IPart>()))
                .Where(p => p is T)
                .Cast<T>();
            
            return parts;
        }

        /// <summary>
        /// Вражеский корабль, по которому ведется огонь
        /// </summary>
        public IUnit Target { get; set; }
        /// <summary>
        /// Список всех креплений
        /// </summary>
        public IEnumerable<Binding> Bindings => m_Bindings;
        /// <summary>
        /// При уничтожение корабля вызывается событие
        /// За непосредственное уничтожение объекта отвечает менеджер
        /// </summary>
        public void Destroy()
        {
            OnDestroy?.Invoke(this);
        }

        #endregion
        #region IUpdateble
        /// <summary>
        /// Вызывает Update у всех установленных устройств которые поддерживают интерфейс IUpdateble
        /// </summary>
        public void Update(float delta)
        {
            var parts = FindParts<IUpdateble>();
            foreach (var iter in parts)
            {
                iter.Update(delta);
            }
        }
        #endregion
    }
*/
}
