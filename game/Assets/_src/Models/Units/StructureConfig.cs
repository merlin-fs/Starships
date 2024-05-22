using System;
using System.Threading.Tasks;

using Common.Core;
using Common.Defs;

using Game.Model.Worlds;

using Unity.Entities;

using UnityEngine;

namespace Game.Model.Units
{
    public class StructureConfig : Config, IViewPrefab
    {
        private Structure.StructureDef Def { get; }
        private readonly IViewPrefab m_ViewPrefab;
        
        public StructureConfig(ObjectID id, IViewPrefab prefab, Structure.StructureDef def) : base(id)
        {
            Def = def;
            m_ViewPrefab = prefab;
        }

        protected override void Configure(Entity root, IDefinableContext context)
        {
            Def.AddComponentData(root, context);
        }

        public Task<GameObject> GetViewPrefab()
        {
            return m_ViewPrefab.GetViewPrefab();
        }
    }
}