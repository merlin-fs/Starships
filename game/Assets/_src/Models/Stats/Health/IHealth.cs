using System;

namespace Game.Model.Stats
{
    public interface IHealth: IStat
    {
        bool IsAlive { get; }
    }
}