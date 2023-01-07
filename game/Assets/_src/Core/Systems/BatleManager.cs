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
    }


    [UpdateInGroup(typeof(GameLogicDoneSystemGroup))]
    public partial struct BatleManager : ISystem
    {
        EntityQuery m_Query;
        ComponentLookup<WorldTransform> m_LookupTransforms;
        BufferLookup<Stat> m_LookupStats;

        EntityQuery m_QueryTargets;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Weapon>()
                .WithAll<Logic>()
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

        public void OnUpdate(ref SystemState state)
        {
            m_LookupTransforms.Update(ref state);
            m_LookupStats.Update(ref state);
            var entities = m_QueryTargets.ToEntityListAsync(Allocator.TempJob, state.Dependency, out JobHandle handle);

            var ecb = state.World.GetExistingSystemManaged<GameLogicCommandBufferSystem>().CreateCommandBuffer();
            var job = new WeaponJob()
            {
                Entities = entities,
                LookupStats = m_LookupStats,
                LookupTransforms = m_LookupTransforms,
                Writer = ecb.AsParallelWriter(),
            };
            state.Dependency = job.ScheduleParallel(m_Query, handle);
            state.Dependency.Complete();
            entities.Dispose(state.Dependency);
        }

        partial struct WeaponJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;
            //[NoAlias]
            [NativeDisableParallelForRestriction]
            [NativeDisableContainerSafetyRestriction]
            public BufferLookup<Stat> LookupStats;
            [ReadOnly] public ComponentLookup<WorldTransform> LookupTransforms;
            [ReadOnly] public NativeList<Entity> Entities;

            void Execute([EntityIndexInQuery] int idx, in WeaponAspect weapon, ref LogicAspect logic)
            {
                if (logic.Equals(Weapon.State.Shoot))
                {
                    if (weapon.Bullet.Def.DamageTargets == DamageTargets.AoE)
                    {
                        AoE(idx, weapon.Self, weapon.Target.Value, weapon);
                    }
                    else
                    {
                        Damage(idx, weapon.Target.Value, weapon);
                    }
                    logic.SetResult(Weapon.Result.Done);
                }
            }

            void Damage(int idx, Entity target, in WeaponAspect weapon)
            {
                if (!LookupStats.HasBuffer(target)) return;
                var stats = LookupStats[target];
                var damage = weapon.Stat(Weapon.Stats.Damage);
                stats.GetRW(GlobalStat.Health).Damage(damage.Value);
                /*
                var damage = new Damage
                {
                    DamageType = weapon.Bullet.Def.DamageType.ID,
                    Targets = weapon.Bullet.Def.DamageTargets,
                    Value = weapon.Stat(Weapon.Stats.Damage).Value,
                };
                Writer.AddComponent(idx, target, damage);
                */
            }

            void AoE(int idx, Entity self, Entity center, in WeaponAspect weapon)
            {
                using var targets = new NativeList<Entity>(Allocator.TempJob);
                FindEnemy(center, weapon.Bullet.Def.Range, LookupTransforms, targets);
                
                foreach(var target in targets)
                {
                    if (target == self) continue;
                    Damage(idx, target, weapon);
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
