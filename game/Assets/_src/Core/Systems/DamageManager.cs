using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;
using Common.Core;
using Common.Repositories;
using Common.Defs;


namespace Game.Model.Weapons
{
    using Stats;
    using Core.Repositories;

    public struct Damage: IBufferElementData
    {
        public ObjectID DamageType;
        public DamageTargets Targets;
        public float Value;
        public float Range;
    }


    [UpdateInGroup(typeof(GameLogicDoneSystemGroup))]
    public partial struct DamageManager : ISystem
    {
        EntityQuery m_Query;
        ComponentLookup<WorldTransform> m_LookupTransforms;
        BufferLookup<Stat> m_LookupStats;
        EntityQuery m_QueryTargets;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Damage>()
                .WithNone<DeadTag>()
                .Build();
            state.RequireForUpdate(m_Query);

            m_QueryTargets = SystemAPI.QueryBuilder()
                .WithAll<Team>()
                .WithNone<DeadTag>()
                .Build();
            m_LookupTransforms = state.GetComponentLookup<WorldTransform>(true);
            m_LookupStats = state.GetBufferLookup<Stat>(false);
        }

        public void OnDestroy(ref SystemState state) { }

        public unsafe void OnUpdate(ref SystemState state)
        {
            m_LookupTransforms.Update(ref state);
            m_LookupStats.Update(ref state);

            var entities = m_QueryTargets.ToEntityListAsync(Allocator.TempJob, state.Dependency, out JobHandle jobHandle);
            var ecb = state.World.GetExistingSystemManaged<GameLogicCommandBufferSystem>().CreateCommandBuffer();
            var repo = Repositories.Instance.ConfigsAsync().Result;
            UnsafeUtility.PinGCObjectAndGetAddress(repo, out ulong handle);

            var job = new WeaponJob()
            {
                Handle = new IntPtr((long)handle),
                Entities = entities,
                LookupStats = m_LookupStats,
                LookupTransforms = m_LookupTransforms,
                Writer = ecb.AsParallelWriter(),
            };
            state.Dependency = job.ScheduleParallel(m_Query, jobHandle);
            state.Dependency = new ClearJob
            {
                Handle = new IntPtr((long)handle),
            }.Schedule(state.Dependency);

            state.Dependency.Complete();
            entities.Dispose(state.Dependency);
        }

        struct ClearJob: IJob
        {
            [NativeDisableUnsafePtrRestriction]
            public IntPtr Handle;

            public void Execute() 
            {
                UnsafeUtility.ReleaseGCObject((ulong)Handle.ToInt64());
            }
        }

        partial struct WeaponJob : IJobEntity
        {
            [NativeDisableUnsafePtrRestriction]
            public IntPtr Handle;
            IReadonlyRepository<ObjectID, IConfig> m_Repo => (IReadonlyRepository<ObjectID, IConfig>)GCHandle.FromIntPtr(Handle).Target;


            public EntityCommandBuffer.ParallelWriter Writer;

            [NativeDisableParallelForRestriction]
            [NativeDisableContainerSafetyRestriction]
            public BufferLookup<Stat> LookupStats;
            [ReadOnly] public ComponentLookup<WorldTransform> LookupTransforms;
            [ReadOnly] public NativeList<Entity> Entities;

            void Execute([EntityIndexInQuery] int idx, in Entity entity, ref DynamicBuffer<Damage> damages)
            {
                foreach (var damage in damages)
                {
                    if (damage.Targets == DamageTargets.AoE)
                    {
                        AoE(idx, entity, damage);
                    }
                    else
                    {
                        Damage(idx, entity, damage);
                    }
                }
                damages.Clear();
            }

            void Damage(int idx, Entity target, Damage damage)
            {
                if (!LookupStats.HasBuffer(target)) return;
                var stats = LookupStats[target];
                var damageType = m_Repo.FindByID(damage.DamageType);

                stats.GetRW(GlobalStat.Health).Damage(damage.Value);
            }

            void AoE(int idx, Entity center, Damage damage)
            {
                using var targets = new NativeList<Entity>(Allocator.TempJob);
                FindEnemy(center, damage.Range, LookupTransforms, targets);

                foreach(var target in targets)
                {
                    Damage(idx, target, damage);
                }
            }

            struct TempFindTarget
            {
                public Entity Entity;
                public float Magnitude;
                public WorldTransform Transform;
            }

            public void FindEnemy(Entity center, float radius, ComponentLookup<WorldTransform> transforms, NativeList<Entity> targets)
            {
                targets.Capacity = Entities.Length;
                if (!transforms.HasComponent(center))
                    return;

                var selfPosition = transforms[center].Position;
                var entities = Entities;
                var writer = targets.AsParallelWriter();

                Parallel.For(0, entities.Length, (i) =>
                {
                    var target = entities[i];
                    var targetPos = transforms[target].Position;
                    var magnitude = (selfPosition - targetPos).magnitude();

                    if (utils.SpheresIntersect(selfPosition, radius, targetPos, 0f, out float3 vector))
                    {
                        writer.AddNoResize(target);
                    }
                });
            }
        }
    }
}
