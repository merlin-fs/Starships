using System;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;
using Common.Defs;


namespace Game.Model.Weapons
{
    using Stats;

    public struct DamageItems: IBufferElementData
    {
        public Entity Sender;
        public float Value;
    }


    [UpdateInGroup(typeof(GameLogicDoneSystemGroup))]
    public partial struct DamageManager : ISystem
    {
        EntityQuery m_Query;
        ComponentLookup<WorldTransform> m_LookupTransforms;
        BufferLookup<Stat> m_LookupStats;
        ComponentLookup<Bullet> m_LookupBullets;
        EntityQuery m_QueryTargets;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<DamageItems>()
                .WithNone<DeadTag>()
                .Build();
            state.RequireForUpdate(m_Query);

            m_QueryTargets = SystemAPI.QueryBuilder()
                .WithAll<Team>()
                .WithNone<DeadTag>()
                .Build();
            m_LookupTransforms = state.GetComponentLookup<WorldTransform>(true);
            m_LookupStats = state.GetBufferLookup<Stat>(false);
            m_LookupBullets = state.GetComponentLookup<Bullet>(true);
        }

        public void OnDestroy(ref SystemState state) { }

        public unsafe void OnUpdate(ref SystemState state)
        {
            m_LookupTransforms.Update(ref state);
            m_LookupStats.Update(ref state);
            m_LookupBullets.Update(ref state);

            var entities = m_QueryTargets.ToEntityListAsync(Allocator.TempJob, state.Dependency, out JobHandle jobHandle);
            var ecb = state.World.GetExistingSystemManaged<GameLogicCommandBufferSystem>().CreateCommandBuffer();

            var job = new WeaponJob()
            {
                Entities = entities,
                LookupStats = m_LookupStats,
                LookupTransforms = m_LookupTransforms,
                LookupBullets = m_LookupBullets,
                Writer = ecb.AsParallelWriter(),
            };
            state.Dependency = job.ScheduleParallel(m_Query, jobHandle);
            state.Dependency.Complete();

            entities.Dispose(state.Dependency);
        }

        partial struct WeaponJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;

            [NativeDisableParallelForRestriction]
            [NativeDisableContainerSafetyRestriction]
            public BufferLookup<Stat> LookupStats;

            [ReadOnly] public ComponentLookup<WorldTransform> LookupTransforms;
            [ReadOnly] public NativeList<Entity> Entities;
            [ReadOnly] public ComponentLookup<Bullet> LookupBullets;

            void Execute([EntityIndexInQuery] int idx, in Entity entity, ref DynamicBuffer<DamageItems> damages)
            {
                var context = new DefExt.WriterContext(Writer, idx);
                foreach (var damage in damages)
                {
                    if (!LookupBullets.HasComponent(damage.Sender)) continue;
                    var bullet = LookupBullets[damage.Sender];

                    var damageConfig = bullet.Def.DamageType;

                    foreach(var iter in damageConfig.Damages)
                    {
                        if (iter.Targets == DamageTargets.AoE)
                        {
                            AoE(idx, entity, iter, bullet.Range, damage.Value, context);
                        }
                        else
                        {
                            Damage(idx, entity, iter, damage.Value, context);
                        }

                    }
                }
                damages.Clear();
            }

            void Damage(int idx, Entity target, IDamage damage, float value, IDefineableContext context)
            {
                if (!LookupStats.HasBuffer(target)) return;
                var stats = LookupStats[target];

                damage.Apply(ref stats, value, context);
            }

            void AoE(int idx, Entity center, IDamage damage, float range, float value, IDefineableContext context)
            {
                using var targets = new NativeList<Entity>(Allocator.TempJob);
                FindEnemy(center, range, LookupTransforms, targets);

                foreach(var target in targets)
                {
                    Damage(idx, target, damage, value, context);
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
