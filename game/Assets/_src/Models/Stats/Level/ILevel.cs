using System;

namespace Game.Model.Stats
{
    /// <summary>
    /// Интерфейс уровня игровой сущности
    /// Является модификатором (IModifier) для изменения статов
    /// </summary>
    public interface ILevel: IModifier
    {
        /// <summary>
        /// Значение уровня
        /// </summary>
        int Value { get; }
        /// <summary>
        /// Возвращает интерфейс для изменения значений
        /// </summary>
        ILevelWritable GetWritable();
    }

    /// <summary>
    /// Интерфейс изменения значений
    /// </summary>
    public interface ILevelWritable : ILevel, IDisposable
    {
        /// <summary>
        /// Увеличивает уровень
        /// </summary>
        void Inc(int value);
        /// <summary>
        /// Устанавливае уровень
        /// </summary>
        void SetValue(int value);
    }
}