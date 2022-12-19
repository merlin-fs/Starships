using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Model.Units
{
    using Stats;
    /*
    /// <summary>
    /// Конфиг корабля
    /// </summary>
    [CreateAssetMenu(fileName = "Unit", menuName = "Configs/Unit")]
    public class UnitConfig: ConfigScriptable<Unit>
    {
        /// <summary>
        /// Реализация IView, для визуализации корабля
        /// </summary>
        public GameObject Prefab;

        /// <summary>
        /// Максимальное значение HP
        /// </summary>
        public float Health;
        /// <summary>
        /// Конфиг уровня
        /// </summary>
        public LevelConfig Level;

        /// <summary>
        /// Максимальная скорость корабля (без учета уровня)
        /// </summary>
        [Header("Ship stats")]
        public float Speed;

        /// <summary>
        /// Список креплений
        /// </summary>
        [Header("Ship bindings")]
        [SerializeReference, Reference(typeof(Binding))]
        public List<Binding> Bindings = new List<Binding>();

        /// <summary>
        /// Создает корабль и IView для него
        /// </summary>
        /// <typeparam name="T">Конечный тип который хотим получить. Должен поддепживать IEntity</typeparam>
        /// <param name="view">Возвращает созданый IView</param>
        /// <returns>Возвращает созданный объект</returns>
        public T Create<T>(out IView view)
        {
            Unit entity = base.Create<Unit>(null);

            var obj = GameObject.Instantiate(Prefab);
            view = obj.GetComponent<IView>();
            view.Init(entity);
            
            return (T)(IEntity)entity;
        }
    }
    */
}
