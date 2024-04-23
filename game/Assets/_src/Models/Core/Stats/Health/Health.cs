using System;

using Game.Core;

using Unity.Entities;

namespace Game.Model.Stats
{
    using static Logics.Logic;

    public struct DeadTag: IComponentData { }

    public struct Global : IStateData
    {
        [EnumHandle]
        public enum State
        {
            Dead,
        }

        [EnumHandle]
        public enum Stats
        {
            Health,
        }
    }
}