using System;
using Unity.Entities;

namespace Game.Model.Units
{
    public readonly partial struct UnitAspect: IAspect
    {
        public readonly Entity Self;

        readonly RefRW<Unit> m_Unit;
    }
}
