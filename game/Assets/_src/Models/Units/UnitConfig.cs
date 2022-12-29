using System;
using Common.Defs;
using UnityEngine;

namespace Game.Model.Units
{
    /// <summary>
    /// Конфиг корабля
    /// </summary>
    [CreateAssetMenu(fileName = "Unit", menuName = "Configs/Unit")]
    public class UnitConfig: ScriptableConfig
    {
        public Unit.UnitConfig Value = new Unit.UnitConfig();
    }
}
