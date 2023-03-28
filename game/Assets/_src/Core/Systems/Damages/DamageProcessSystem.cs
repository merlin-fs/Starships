using System;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Common.Defs;
using Common.Core;


namespace Game.Model.Weapons
{
    using Stats;

    public partial struct Damage
    {
        public struct LastDamage : IBufferElementData
        {
            public ObjectID DamageConfigID;
            public float Value;
        }

        [UpdateInGroup(typeof(GameLogicEndSystemGroup))]
        partial struct DamageProcessSystem : ISystem
        {
            EntityQuery m_Query;
            ComponentLookup<DeadTag> m_LookupDead;
            ComponentLookup<LocalTransform> m_LookupTransforms;
            EntityQuery m_QueryTargets;
            StatAspect.Lookup m_LookupStatAspect;

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAll<Data>()
                    .Build();
                state.RequireForUpdate(m_Query);

                m_QueryTargets = SystemAPI.QueryBuilder()
                    .WithNone<DeadTag>()
                    .WithAspectRO<StatAspect>()
                    .Build();

                m_LookupDead = state.GetComponentLookup<DeadTag>(true);
                m_LookupTransforms = state.GetComponentLookup<LocalTransform>(true);
                m_LookupStatAspect = new StatAspect.Lookup(ref state, false);
            }

            public unsafe void OnUpdate(ref SystemState state)
            {
                m_LookupDead.Update(ref state);
                m_LookupTransforms.Update(ref state);
                m_LookupStatAspect.Update(ref state);

                var entities = m_QueryTargets.ToEntityListAsync(Allocator.TempJob, state.Dependency, out JobHandle jobHandle);
                var system = state.World.GetExistingSystemManaged<GameLogicEndCommandBufferSystem>();
                var ecb = system.CreateCommandBuffer();

                var job = new SystemJob()
                {
                    LookupEntity = state.GetEntityStorageInfoLookup(),
                    Entities = entities,
                    LookupDead = m_LookupDead,
                    LookupStatAspect = m_LookupStatAspect,
                    LookupTransforms = m_LookupTransforms,
                    Writer = ecb.AsParallelWriter(),
                };
                state.Dependency = job.ScheduleParallel(m_Query, jobHandle);
                state.Dependency.Complete();
                ecb.DestroyEntity(m_Query);
                entities.Dispose(state.Dependency);
            }

            partial struct SystemJob : IJobEntity
            {
                public EntityStorageInfoLookup LookupEntity;
                public EntityCommandBuffer.ParallelWriter Writer;
                [NativeDisableParallelForRestriction]
                public StatAspect.Lookup LookupStatAspect;
                [ReadOnly] public ComponentLookup<DeadTag> LookupDead;
                [ReadOnly] public ComponentLookup<LocalTransform> LookupTransforms;
                [ReadOnly] public NativeList<Entity> Entities;

                public void Execute([EntityIndexInQuery] int idx, in Data damage)
                {
                    var damageConfig = damage.Bullet.Def.DamageType;
                    foreach (var iter in damageConfig.Damages)
                    {
                        if (damage.Target.Value != Entity.Null && !LookupEntity.Exists(damage.Target.Value))
                            return;

                        var pos = damage.Target.Value != Entity.Null
                            ? LookupTransforms[damage.Target.Value]
                            : damage.SenderTransform;

                        if (damageConfig.Targets == DamageTargets.AoE)
                        {
                            AoE(idx, damage.Sender, damageConfig.ID, pos.Position, iter, damage.Bullet.Range, damage.Value);
                        }
                        else
                        {
                            Damage(idx, damageConfig.ID, damage.Target.Value, iter, damage.Value);
                        }
                    }
                }

                void Damage(int idx, ObjectID cfgID, Entity target, IDamage damage, float value)
                {
                    if (!LookupEntity.Exists(target))
                    {
                        UnityEngine.Debug.LogError($"{target} [Damage] not exists");
                        return;
                    }

                    if (LookupDead.HasComponent(target))
                        return;
                    UnityEngine.Debug.Log($"{target} [Damage] try lookup");
                    var stat = LookupStatAspect[target];

                    if (LookupDead.HasComponent(stat.Self) || LookupDead.HasComponent(stat.Root))
                    {
                        return;
                    }
                    damage.Apply(ref stat, value);
                    Writer.AppendToBuffer(idx, target, new LastDamage() { DamageConfigID = cfgID, Value = value });
                }

                void AoE(int idx, Entity self, ObjectID cfgID, float3 center, IDamage damage, float range, float value)
                {
                    using var targets = new NativeList<Entity>(1000, Allocator.TempJob);
                    FindEnemy(self, center, range, LookupTransforms, targets);

                    foreach (var target in targets)
                    {
                        Damage(idx, cfgID, target, damage, value);
                    }
                }

                public void FindEnemy(Entity self, float3 center, float radius, ComponentLookup<LocalTransform> transforms, NativeList<Entity> targets)
                {
                    targets.Capacity = Entities.Length;
                    var entities = Entities;
                    var writer = targets.AsParallelWriter();
                    var lookupStatAspect = LookupStatAspect;

                    Parallel.For(0, entities.Length, (i) =>
                    {
                        var target = entities[i];
                        var stat = lookupStatAspect[target];
                        //if (stat.Self == self || stat.Root.Value == self)
                        //    return;
                        var targetPos = transforms[target].Position;
                        var magnitude = (center - targetPos).magnitude();

                        if (utils.SpheresIntersect(center, radius, targetPos, 0f, out float3 vector))
                        {
                            writer.AddNoResize(target);
                        }
                    });
                }
            }
        }
    }
}
