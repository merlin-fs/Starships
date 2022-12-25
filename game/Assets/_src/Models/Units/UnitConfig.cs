using System;
using UnityEngine;

namespace Game.Model.Units
{
    /// <summary>
    /// Конфиг корабля
    /// </summary>
    [CreateAssetMenu(fileName = "Unit", menuName = "Configs/Unit")]
    public class UnitConfig: ScriptableObject
    {
        public Unit.UnitConfig Value = new Unit.UnitConfig();
        public GameObject Prefab;
    }
}
