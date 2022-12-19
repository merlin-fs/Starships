using System;

namespace Game.Model
{
    /// <summary>
    /// Интерфейс модификатора Урона 
    /// </summary>
    public interface IModifierDamage
    {
        /// <summary>
        /// Расчитывает изменение урона
        /// </summary>
        /// <param name="value">Урон</param>
        /// <param name="damageType">Тип урона</param>
        void Сalculation(ref float value, DamageType damageType);
    }
}
