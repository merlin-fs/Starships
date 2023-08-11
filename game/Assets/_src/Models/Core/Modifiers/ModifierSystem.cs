using System;
using System.Collections.Concurrent;
using Unity.Collections;
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

        public ulong AddModifier<T, S>(Entity entity, ref T modifier, S statType)
            where T: struct, IModifier
            where S: struct, IConvertible
        {
            var mod = Modifier.Create(ref modifier, statType);
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

        struct ModifyJob: IJobParallelFor
        {
            public NativeArray<Item> Items;
            [NativeDisableParallelForRestriction]
            public BufferLookup<Modifier> LookupModifiers;
            
            public void Execute(int index)
            {
                var iter = Items[index];
                try
                {
                    var modifiers = LookupModifiers[iter.Entity];
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
            }
        }

        protected override void OnUpdate()
        {
            if (m_Queue.Count == 0)
                return;
            m_LookupModifiers.Update(ref CheckedStateRef);

            var items = new NativeArray<Item>(m_Queue.ToArray(), Allocator.TempJob);
            m_Queue.Clear();

            Dependency = new ModifyJob
            {
                Items = items,
                LookupModifiers = m_LookupModifiers,
            }.Schedule(items.Length, 5, Dependency);
            items.Dispose(Dependency);
        }
    }
}