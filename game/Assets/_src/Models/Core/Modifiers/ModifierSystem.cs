using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace Game.Model.Stats
{
    [UpdateInGroup(typeof(GameLogicInitSystemGroup))]
    public partial class ModifiersSystem : SystemBase
    {
        public static ModifiersSystem Instance { get; private set; }

        List<ItemInfo> m_Handles;
        NativeQueue<Item> m_Queue;

        private struct ItemInfo
        {
            public Task Task;
            public EventWaitHandle Event;
        }

        private struct Item
        {
            public int Index;
            public Entity Entity;
            public Modifier Modifier;
            public IntPtr EventHandle;
        }

        protected override void OnCreate()
        {
            Instance = this;
            m_Queue = new NativeQueue<Item>(Allocator.Persistent);
            m_Handles = new List<ItemInfo>();
        }

        protected override void OnDestroy()
        {
            ClearHandles();
            m_Queue.Dispose();
        }

        public Task<int> AddModifier<T>(Entity entity, ref T modifier, Enum statType)
            where T: struct, IModifier
        {
            var mod = Modifier.Create<T>(ref modifier, statType);
            var @event = new ManualResetEvent(false);
            var info = new ItemInfo()
            {
                Event = @event,
            };

            var item = new Item()
            {
                Index = -1,
                Modifier = mod,
                Entity = entity,
                EventHandle = @event.SafeWaitHandle.DangerousGetHandle(),
            };

            m_Queue
                .AsParallelWriter()
                .Enqueue(item);


            var task = Task<int>.Run(() =>
            {
                while (!@event.SafeWaitHandle.IsClosed && !@event.SafeWaitHandle.IsInvalid)
                {
                    if (@event.WaitOne(1))
                    {
                        int index = m_Results[mod.UID];
                        return index;
                    }
                }
                return -1;
            });
            info.Task = task;
            m_Handles.Add(info);
            return task;
        }

        public Task DelModifier(Entity entity, int index)
        {
            ulong id = (ulong)index;
            var @event = new ManualResetEvent(false);
            var info = new ItemInfo()
            {
                Event = @event,
            };

            var item = new Item()
            {
                Index = index,
                Modifier = default,
                Entity = entity,
                EventHandle = @event.SafeWaitHandle.DangerousGetHandle(),
            };
            m_Queue
                .AsParallelWriter()
                .Enqueue(item);

            info.Task = Task.Run(() =>
            {
                while (!@event.SafeWaitHandle.IsClosed && !@event.SafeWaitHandle.IsInvalid)
                {
                    if (@event.WaitOne(1))
                        return;
                }
            });
            m_Handles.Add(info);
            return info.Task;
        }

        protected override void OnUpdate()
        {
            if (m_Queue.Count == 0)
                return;
            using var items = m_Queue.ToArray(Allocator.Persistent);
            m_Queue.Clear();
            
            var writer = m_Results;
            foreach (var iter in items)
            {
                using (EventWaitHandle waitEvent = new EventWaitHandle(false, EventResetMode.ManualReset))
                {
                    var aspect = EntityManager.GetAspect<ModifiersAspect>(iter.Entity);
                    if (iter.Index < 0)
                    {
                        var id = aspect.AddModifier(iter.Modifier);
                        waitEvent.SafeWaitHandle = new SafeWaitHandle(iter.EventHandle, false);
                        writer.Add(iter.Modifier.UID, id);
                        waitEvent.Set();
                    }
                    else
                    {
                        aspect.DelModifier(iter.Index);
                        waitEvent.SafeWaitHandle = new SafeWaitHandle(iter.EventHandle, false);
                        waitEvent.Set();
                    }
                }
            }

            Task.WaitAll(m_Handles.Select(i => i.Task).ToArray());
            ClearHandles();
            m_Results.Clear();
        }

        private void ClearHandles()
        {
            if (m_Handles.Count > 0)
            {
                foreach (var iter in m_Handles)
                    iter.Event.Dispose();
                m_Handles.Clear();
            }
        }
    }
}