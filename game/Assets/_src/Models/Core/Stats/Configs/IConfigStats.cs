using System;
using Unity.Entities;
using Common.Core;

namespace Game.Core.Defs
{ 
    using Model.Stats;

    public struct PrepareStat: IBufferElementData
    {
        public ObjectID ConfigID;
    }

    public interface IConfigStats
    {
        void Configurate(DynamicBuffer<Stat> stats);
    }
}
