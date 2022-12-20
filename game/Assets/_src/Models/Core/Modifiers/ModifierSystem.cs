using System;
using Unity.Collections;
using Unity.Entities;

namespace Game.Model.Stats
{
    [UpdateInGroup(typeof(GameLogicInitSystemGroup))]
    public partial class ModifiersSystem : SystemBase
    {
        public static ModifiersSystem Instance { get; private set; }

        NativeQueue<Item> m_Queue;

        private struct Item
        {
            public uint UID;
            public Entity Entity;
            public Modifier Modifier;
        }

        protected override void OnCreate()
        {
            Instance = this;
            m_Queue = new NativeQueue<Item>(Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            m_Queue.Dispose();
        }

        public unsafe void AddModifier<T>(Entity entity, ref T modifier, uint uid, Enum statType)
            where T : IModifier
        {
            m_Queue
                .AsParallelWriter()
                .Enqueue(new Item()
                {
                    UID = 0,
                    Modifier = Modifier.Create(ref modifier, uid, statType),
                    Entity = entity,
                });
        }

        public unsafe void DelModifier(Entity entity, uint uid)
        {
            m_Queue
                .AsParallelWriter()
                .Enqueue(new Item()
                {
                    UID = uid,
                    Modifier = default,
                    Entity = entity,
                });
        }

        protected override void OnUpdate()
        {
            if (m_Queue.Count == 0)
                return;

            using var items = m_Queue.ToArray(Allocator.Temp);
            m_Queue.Clear();

            foreach (var iter in items)
            {
                var aspect = EntityManager.GetAspect<ModifiersAspect>(iter.Entity);
                if (iter.UID == 0)
                    aspect.AddModifier(iter.Modifier);
                else
                    aspect.DelModifier(iter.UID);
            }
        }
    }
}