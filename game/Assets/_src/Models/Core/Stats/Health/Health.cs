using System;
using Unity.Entities;

namespace Game.Model.Stats
{
    public struct DeadTag: IComponentData { }

    public enum GlobalAction
    {
        Destroy,
    }

    public enum GlobalStat
    {
        Health,
    }
}