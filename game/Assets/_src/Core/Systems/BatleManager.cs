using System;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;
using Common.Core;

namespace Game.Model.Weapons
{
    using Stats;
    using Logics;

    public struct Damage: IComponentData
    {
        public ObjectID DamageType;
        public DamageTargets Targets;
        public float Value;
        public float Range;
    }


    [UpdateInGroup(typeof(GameLogicDoneSystemGroup))]
    public partial struct BatleManager : ISystem
    {
        EntityQuery m_Query;
        ComponentLookup<WorldTransform> m_LookupTransforms;
        BufferLookup<Stat> m_LookupStats;
        ComponentLookup<Weapon> m_LookupWeapon;

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
            m_LookupWeapon = state.GetComponentLookup<Weapon>(true);
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            m_LookupTransforms.Update(ref state);
            m_LookupStats.Update(ref state);
            m_LookupWeapon.Update(ref state);

            var entities = m_QueryTargets.ToEntityListAsync(Allocator.TempJob, state.Dependency, out JobHandle handle);

            var ecb = state.World.GetExistingSystemManaged<GameLogicCommandBufferSystem>().CreateCommandBuffer();
            var job = new WeaponJob()
            {
                Entities = entities,
                LookupStats = m_LookupStats,
                LookupTransforms = m_LookupTransforms,
                LookupWeapon = m_LookupWeapon,
                Writer = ecb.AsParallelWriter(),
            };
            state.Dependency = job.ScheduleParallel(m_Query, handle);
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
            [ReadOnly] public ComponentLookup<Weapon> LookupWeapon;

            void Execute([EntityIndexInQuery] int idx, in Entity entity, in Damage damage)
            {
                if (damage.Targets == DamageTargets.AoE)
                {
                    AoE(idx, entity, damage);
                }
                else
                {
                    Damage(idx, entity, damage);
                }
                Writer.RemoveComponent<Damage>(idx, entity);
            }

            void Damage(int idx, Entity target, Damage damage)
            {
                if (!LookupStats.HasBuffer(target)) return;
                var stats = LookupStats[target];
                //var damage = weapon.Stat(Weapon.Stats.Damage);
                stats.GetRW(GlobalStat.Health).Damage(damage.Value);
            }

            void AoE(int idx, Entity center, Damage damage)
            {
                using var targets = new NativeList<Entity>(Allocator.TempJob);
                FindEnemy(center, damage.Range, LookupTransforms, targets);
                
                foreach(var target in targets)
                {
                    //if (target == self) continue;
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
