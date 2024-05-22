using Game.Model.Worlds;

using Unity.Entities;

namespace Buildings
{
    public partial class Editor
    {
        public struct Selected : IComponentData
        {
            public Map.Move Position;
        }

        public struct Spawn : IComponentData
        {
            public int Index;
        }
    }
}
