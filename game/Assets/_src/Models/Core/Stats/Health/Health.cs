using System;
using Unity.Entities;

namespace Game.Model.Stats
{
    using static Logics.Logic;

    public struct DeadTag: IComponentData { }

    public struct Global : IStateData
    {
        public enum Action
        {
            Destroy,
        }

        public enum State
        {
            Dead,
        }

        public enum Stat
        {
            Health,
        }
    }
}