using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Jobs;

namespace Game.Model.Stats
{
    [UpdateInGroup(typeof(GameLogicInitSystemGroup))]
    public partial class ModifiersSystem : SystemBase
    {
        public static ModifiersSystem Instance { get; private set; }
        ConcurrentQueue<Item> m_Queue;

        private BufferLookup<Modifier> m_LookupModifiers;

        private struct Item
        {
            public ulong UID;
            public Entity Entity;
            public Modifier Modifier;
        }

        protected override void OnCreate()
        {
            Instance = this;
            //m_Queue = new NativeQueue<Item>(Allocator.Persistent);
            m_Queue = new ConcurrentQueue<Item>();
            m_LookupModifiers = GetBufferLookup<Modifier>(false);
        }

        protected override void OnDestroy()
        {
            //m_Queue.Dispose();
        }

        public ulong AddModifier<T>(Entity entity, ref T modifier, Enum statType)
            where T: struct, IModifier
        {
            var mod = Modifier.Create<T>(ref modifier, statType);
            var item = new Item()
            {
                UID = 0,
                Modifier = mod,
                Entity = entity,
            };
            m_Queue.Enqueue(item);
            /*
            m_Queue
                //.AsParallelWriter()
                .Enqueue(item);
            */
            return mod.UID;
        }

        public void DelModifier(Entity entity, ulong uid)
        {
            var item = new Item()
            {
                UID = uid,
                Modifier = default,
                Entity = entity,
            };
            m_Queue.Enqueue(item);
            /*
            m_Queue
                //.AsParallelWriter()
                .Enqueue(item);
            */
        }

        protected override void OnUpdate()
        {
            if (m_Queue.Count == 0)
                return;

            m_LookupModifiers.Update(ref CheckedStateRef);
            /*
            using var items = m_Queue.ToArray(Allocator.Temp);
            m_Queue.Clear();
            */
            var items = m_Queue.ToArray();
            m_Queue.Clear();

            Parallel.ForEach(items, (iter) =>
            {
                try
                {
                    var modifiers = m_LookupModifiers[iter.Entity];
                    //SystemAPI.GetAspectRW<StatAspect>(iter.Entity);
                    //var stats = EntityManager.GetAspect<StatAspect>(iter.Entity);
                    if (iter.UID == 0)
                    {
                        Modifier.AddModifier(iter.Modifier, ref modifiers);
                    }
                    else
                    {
                        Modifier.DelModifier(iter.UID, ref modifiers);
                    }
                }
                catch (Exception e)
                {
                    if (iter.UID == 0)
                        UnityEngine.Debug.LogError($"add {iter.Entity}");
                    else
                        UnityEngine.Debug.LogError($"del {iter.Entity}");
                    throw e;
                }
            });
            //items.Dispose();
        }
    }
}