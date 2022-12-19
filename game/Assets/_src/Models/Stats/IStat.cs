using System;
using UnityEngine;

namespace Game.Model.Stats
{
    /// <summary>
    /// Интерфейс статов
    /// </summary>
    public interface IStat
    {
        /// <summary>
        /// Тип стата
        /// </summary>
        Enum StatType { get; }
        /// <summary>
        /// Текущее значение
        /// </summary>
        float Value { get; }
        /// <summary>
        /// Максимальное значение
        /// </summary>
        float MaxValue { get; }
        /// <summary>
        /// Вычисление значения в зависимости от модификаций
        /// </summary>
        void Сalculation();
        /// <summary>
        /// Возвращает интерфейс для изменения значений
        /// </summary>
        IStatWritable GetWritable();
    }

    /// <summary>
    /// Интерфейс изменения значений
    /// </summary>
    public interface IStatWritable : IStat, IDisposable
    {
        /// <summary>
        /// Добавляет модификатор
        /// </summary>
        void AddMod(IModifier mod);
        /// <summary>
        /// Удаляет модификатор
        /// </summary>
        void RemoveMod(IModifier mod);
        /// <summary>
        /// Уменьшает значение
        /// </summary>
        void Dec(float value);
        /// <summary>
        /// Увеличивает значение
        /// </summary>
        void Add(float value);
        /// <summary>
        /// Устанавливает значение
        /// </summary>
        void SetValue(float value);
        /// <summary>
        /// Устанавливает значение в MaxValue
        /// </summary>
        void ResetValue();
    }
}