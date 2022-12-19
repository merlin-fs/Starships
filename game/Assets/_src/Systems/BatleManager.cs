using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common.Singletons;

namespace Game.Systems
{
    using Model;
    using Model.Units;

    /// <summary>
    /// Тестовая реализация BatleManager`а
    /// </summary>
    public class BatleManager : MonoSingleton<BatleManager>
    {
        public static BatleManager Inststance => Inst;

        /// <summary>
        ///Производит "выстрел" по указанной цели
        /// </summary>
        /// <param name="target">Цель</param>
        /// <param name="source">Исходный корабль</param>
        /// <param name="damage">Урон</param>
        /// <param name="damageType">Тип урона</param>
        public void Hot(IUnit target, IUnit source, float damage, DamageType damageType)
        {
            //Ищем все модификаторы у корабля - цели
            var mods = target.FindParts<IModifierDamage>();
            //Вычисляем конечный урон
            foreach(var iter in mods)
            {
                iter.Сalculation(ref damage, damageType);
            }
            
            //Наносим урон HP
            /*
            var health = target.GetStat(GlobalStatType.Health);
            using (var writer = health.GetWritable())
            {
                writer.Dec(damage);
            }
            */
        }
    }
}
