using System;
using Unity.Entities;
using Common.Defs;

namespace Game.Core.Defs
{
    using Model.Stats;

    public interface IConfigStats
    {
        void Configurate(DynamicBuffer<Stat> stats);
    }
}
