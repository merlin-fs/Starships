using System;

namespace Game.Model.Worlds
{
    public partial struct Map
    {
        public struct Bitmask
        {
            public int Value;
            public static implicit operator Bitmask(int value) => new Bitmask { Value = value };
        }
    }
}
