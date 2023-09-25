using System;

using JetBrains.Annotations;

using Unity.Collections;
using Unity.Mathematics;

namespace Game.Model.Worlds
{
    public partial struct Map
    {
        public static NativeParallelHashSet<int2> GetCells(int2 center, int radius, [CanBeNull] Func<int2, bool> isPassable)
        {
            var set = new NativeParallelHashSet<int2>(radius * 8, Allocator.Temp);
            int idx;
            for (int x = center.x - radius; x <= center.x + radius; x = idx)
            {
                for (int y = center.y - radius; y <= center.y + radius; y = idx)
                {
                    if ((center.x - x) * (center.x - x) + (center.y - y) * (center.y - y) <= radius * radius)
                    {
                        var value = new int2(x, y);
                        bool passable = isPassable?.Invoke(value) ?? true;
                        if (passable)
                            set.Add(value);
                    }
                    idx = y + 1;
                }
                idx = x + 1;
            }
            return set;
        }
    }
}