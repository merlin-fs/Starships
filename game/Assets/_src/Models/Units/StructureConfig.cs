using System;
using Common.Core;
using Common.Defs;

using Game.Model.Worlds;

using Unity.Entities;

namespace Game.Model.Units
{
    public class StructureConfig : Config
    {
        public Structure.StructureDef Def { get; }
        
        public StructureConfig(ObjectID id, Entity prefab, Structure.StructureDef def) : base(id, prefab)
        {
            Def = def;
        }
    }
}