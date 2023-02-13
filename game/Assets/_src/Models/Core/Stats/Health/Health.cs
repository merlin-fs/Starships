using System;
using Unity.Entities;

namespace Game.Model.Stats
{
    public struct DeadTag: IComponentData { }

    public enum GlobalState
    {
        Destroy,
    }

    public enum GlobalStat
    {
        Health,
    }
}