using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;

namespace Game.Model.Worlds
{
    public partial struct Map
    {
        public partial struct Data
        {
            public delegate void EnumTile(int x, int y);

            public int2? GetTile(int x, int y, int idx) => GetTile(x, y, (Direct)idx);
            public int2? GetTile(int x, int y, Direct direct)
            {
                var pos = direct.Position(x, y);
                return (pos.x >= 0 && pos.x < Size.x &&
                        pos.y >= 0 && pos.y < Size.y)
                    ? pos
                    : null;
            }

            public int2? GetTile(int2 pos, Direct direct) => GetTile(pos.x, pos.y, direct);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Passable(int2 pos) => Passable(pos.x, pos.y);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Passable(int x, int y)
            {
                return (x >= 0 && x < Size.x && y >= 0 && y < Size.y);
            }

            public (int x, int y) FromIndex(int index)
            {
                int y = index / Size.x;
                int x = index % Size.x;
                return (x, y);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int At(int x, int y) => (y * Size.x) + x;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int At(int2 pos) => At(pos.x, pos.y);

            public int Length => Size.x * Size.y;

            public NativeList<int2> GetNeighbors(int2 source)
            {
                var en = Enum.GetValues(typeof(Map.Direct));
                var list = new NativeList<int2>(en.Length, Allocator.Temp);

                foreach (Map.Direct direct in en)
                {
                    var neighbor = GetTile(source.x, source.y, direct);
                    if (neighbor.HasValue)
                    {
                        list.Add(neighbor.Value);
                    }
                }
                return list;
            }

            public void ParallelForeachTiles(EnumTile action)
            {
                int localY = Size.y;
                Parallel.For(0, Size.x,
                    (x) =>
                    {
                        Parallel.For(0, localY,
                            (y) =>
                            {
                                action(x, y);
                            }
                        );
                    });
            }
        }
    }
}