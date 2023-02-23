using System;
using Unity.Mathematics;

namespace Game.Model.Worlds
{
    public partial struct Map
    {
        public partial struct Data
        {
            public float3 MapToWord(int2 value, float height = 0f)
            {
                float3 pos = new float3(value.x + 0.5f, height, value.y + 0.5f);
                return math.transform(ViewData.LocalToWorldMatrix, pos);
            }

            public float3 MapToWord(int2 value)
            {
                float3 pos = new float3(value.x, 0, value.y);
                float3 offset = -math.transform(ViewData.LocalToWorldMatrix, new float3(Size.x / 2, 0, Size.x / 2));
                pos = math.transform(ViewData.LocalToWorldMatrix, pos) + offset;
                return pos;
            }
        }
    }
}
