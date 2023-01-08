using System;
using Unity.Entities;

namespace Game.Model.Stats
{

    public readonly struct DeadTag: IComponentData { }

    public enum GlobalState
    {
        Destroy,
    }

    public enum GlobalStat
    {
        Health,
    }
}