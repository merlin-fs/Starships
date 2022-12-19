using System;
using System.Collections.Generic;

namespace Game.Model.Units
{
    /// <summary>
    /// Интерфейс юнита (коробля)
    /// </summary>
    public interface IUnit: IUpdateble
    {
        /// <summary>
        /// Ссылка на вражеский корабль
        /// 
        /// если игра по типу FTL, то можо так и оставить
        /// Если игра с множиством кораблей, нужно делать поиск цели
        /// </summary>
        IUnit Target { get; set; }

        /// <summary>
        /// Поиск установленных устройств
        /// 
        /// например: В BatleManager ищем все устройства, которые влияют на урон
        /// target.FindParts<IModifierDamage>();
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEnumerable<T> FindParts<T>();

        /// <summary>
        /// Уничтожает юнит
        /// </summary>
        void Destroy();
    }
}
