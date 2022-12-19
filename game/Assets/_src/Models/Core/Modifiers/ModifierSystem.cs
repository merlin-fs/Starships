using System;
using System.Reflection;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;


namespace Game.Model.Stats
{
    [DisableAutoCreation]
    public partial class ModifierSystem : SystemBase
    {
        private Queue<ModifiersInfo> m_Queue = new Queue<ModifiersInfo>();
        private Dictionary<TypeIndex, ModifiersInfo> m_Modifiers = new Dictionary<TypeIndex, ModifiersInfo>();
        private EntityQuery m_Query;

        private struct ModifiersInfo
        {
            public TypeIndex TypeIndex;
            public IModifyJobWrapper JobWrapper;
            public int Index;
        }

        public static void Registry<T>()
            where T : struct, IModifier, IComponentData
        {
            var system = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<ModifierSystem>();
            var index = TypeManager.GetTypeIndex<T>();
            
            system?.PushData(new ModifiersInfo()
            {
                TypeIndex = index,
                JobWrapper = new ModifyJobWrapper<T>(),
                Index = -1,
            });
        }

        private void PushData(ModifiersInfo info)
        {
            m_Queue.Enqueue(info);
        }

        private void ReBuildQuery(IEnumerable<ModifiersInfo> infos, ref SystemState state)
        {
            foreach (var iter in infos)
            {
                m_Modifiers.Add(iter.TypeIndex, iter);
            }

            using var builder = new EntityQueryBuilder(Allocator.Temp);
            var list = new NativeList<ComponentType>(m_Modifiers.Count, Allocator.Temp);
            try
            {
                foreach (var iter in m_Modifiers)
                {
                    list.Add(ComponentType.FromTypeIndex(iter.Key));
                }
                builder.WithAny(ref list);
                m_Query = builder
                    .WithAll<Modifier>()
                    .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities)
                    .Build(ref state);
            }
            finally
            {
                list.Dispose();
            }
        }

        private void ReBuildQuery(IEnumerable<ModifiersInfo> infos, SystemBase state)
        {
            foreach (var iter in infos)
            {
                m_Modifiers.Add(iter.TypeIndex, iter);
            }

            using var builder = new EntityQueryBuilder(Allocator.Temp);
            var list = new NativeList<ComponentType>(m_Modifiers.Count, Allocator.Temp);
            try
            {
                foreach (var iter in m_Modifiers)
                {
                    list.Add(ComponentType.FromTypeIndex(iter.Key));
                }
                builder.WithAny(ref list);
                m_Query = builder
                    .WithAll<Modifier>()
                    .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities)
                    .Build(state);
            }
            finally
            {
                list.Dispose();
            }
        }

        public void OnCreate(ref SystemState state)
        {
            //m_Queue = new NativeQueue<TypeIndex>(Allocator.Persistent);
            //m_Query = SystemAPI.QueryBuilder()
            //    .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities)
            //    .Build();
            //m_Modifiers = new NativeHashMap<TypeIndex, ModifiersInfo>(10, Allocator.Persistent);
        }

        public void OnDestroy(ref SystemState state)
        {
            //m_Queue.Dispose();
            //m_Modifiers.Dispose();
        }

        interface IModifyJobWrapper
        {
            JobHandle Schedule(EntityQuery query, EntityCommandBuffer.ParallelWriter writer, BufferTypeHandle<Modifier> buffer,
                JobHandle dependsOn, ref SystemState state);
            JobHandle Schedule(EntityQuery query, EntityCommandBuffer.ParallelWriter writer, BufferTypeHandle<Modifier> buffer,
                JobHandle dependsOn, SystemBase state);
        }

        struct ModifyJobWrapper<T> : IModifyJobWrapper
            where T : struct, IModifier, IComponentData
        {
            partial struct ModifyJob : IJobChunk
            {
                [ReadOnly]
                public uint LastSystemVersion;
                [ReadOnly]
                public DynamicComponentTypeHandle DynamicHandle;

                public EntityCommandBuffer.ParallelWriter Writer;

                public BufferTypeHandle<Modifier> Buffer;


                public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
                {
                    if (chunk.DidChange(ref DynamicHandle, LastSystemVersion))
                    {
                        Debug.Log($"DidChange {GetType().ToGenericTypeString()}");

                        BufferAccessor<Modifier> buffers = chunk.GetBufferAccessor(ref Buffer);
                        var array = chunk.GetDynamicComponentDataArrayReinterpret<T>(ref DynamicHandle, UnsafeUtility.SizeOf<T>());
                        ChunkEntityEnumerator enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                        while (enumerator.NextEntityIndex(out var index))
                        {
                            var data = array[index];
                            data.Attach(buffers[index]);
                        }
                    }
                }
            }

            public JobHandle Schedule(EntityQuery query, EntityCommandBuffer.ParallelWriter writer, BufferTypeHandle<Modifier> buffer,
                JobHandle dependsOn, ref SystemState state)
            {
                var job = new ModifyJob()
                {
                    Buffer = buffer,
                    Writer = writer,
                    DynamicHandle = state.GetDynamicComponentTypeHandle(ComponentType.ReadOnly<T>()),
                    LastSystemVersion = state.LastSystemVersion,
                };
                return job.ScheduleParallel(query, dependsOn);
            }

            public JobHandle Schedule(EntityQuery query, EntityCommandBuffer.ParallelWriter writer, BufferTypeHandle<Modifier> buffer,
                JobHandle dependsOn, SystemBase state)
            {
                var job = new ModifyJob()
                {
                    Buffer = buffer,
                    Writer = writer,
                    DynamicHandle = state.GetDynamicComponentTypeHandle(ComponentType.ReadOnly<T>()),
                    LastSystemVersion = state.LastSystemVersion,
                };
                return job.ScheduleParallel(query, dependsOn);
            }
        }



        protected override void OnUpdate()
        //public unsafe void OnUpdate(ref SystemState state)
        {
            if (m_Queue.Count > 0)
            {
                ReBuildQuery(m_Queue, this);
                m_Queue.Clear();
                /*
                using (var types = m_Queue.ToArray(Allocator.Temp))
                {
                    m_Queue.Clear();
                }*/
            }

            if (m_Query.IsEmptyIgnoreFilter)
                return;

            var count = m_Query.CalculateEntityCount() * 2; // Potentially 2x changed: current and previous
            if (count == 0)
                return;

            NativeArray<JobHandle> jobs = new NativeArray<JobHandle>(m_Modifiers.Count, Allocator.Temp);

            int i = 0;
            foreach (var iter in m_Modifiers)
            {
                jobs[i] = iter.Value.JobWrapper.Schedule(m_Query,
                    World.GetExistingSystemManaged<BeginSimulationEntityCommandBufferSystem>().CreateCommandBuffer().AsParallelWriter(),
                    GetBufferTypeHandle<Modifier>(false),
                    Dependency, this);
                i++;
            }
            JobHandle.CompleteAll(jobs);
            jobs.Dispose();
        }
    }
}
