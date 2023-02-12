using System;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;
using Common.Defs;
using Common.Core;


namespace Game.Model.Weapons
{
    using Stats;

    public struct LastDamages: IBufferElementData
    {
        public ObjectID DamageConfigID;
        public float Value;
    }

    [UpdateInGroup(typeof(GameLogicDoneSystemGroup))]
    public partial struct DamageManager : ISystem
    {
        EntityQuery m_Query;
        ComponentLookup<WorldTransform> m_LookupTransforms;
        BufferLookup<Stat> m_LookupStats;
        EntityQuery m_QueryTargets;

        public struct DamageData : IComponentData
        {
            public Entity Sender;
            public Target Target;
            public Bullet Bullet;
            public float Value;
        }

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<DamageData>()
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

        public static void Damage(Entity entity, Target target, Bullet bullet, float value, IDefineableContext context)
        {
            var damageEntity = context.CreateEntity();

            var damage = new DamageData
            {
                Sender = entity,
                Value = value,
                Target = target,
                Bullet = bullet,
            };
            context.AddComponentData(damageEntity, damage);
        }

        public unsafe void OnUpdate(ref SystemState state)
        {
            m_LookupTransforms.Update(ref state);
            m_LookupStats.Update(ref state);

            var entities = m_QueryTargets.ToEntityListAsync(Allocator.TempJob, state.Dependency, out JobHandle jobHandle);
            var system = state.World.GetExistingSystemManaged<GameLogicCommandBufferSystem>();
            var ecb = system.CreateCommandBuffer();

            var job = new WeaponJob()
            {
                Entities = entities,
                LookupStats = m_LookupStats,
                LookupTransforms = m_LookupTransforms,
                Writer = ecb.AsParallelWriter(),
            };
            state.Dependency = job.ScheduleParallel(m_Query, jobHandle);
            state.Dependency.Complete();
            //ecb.DestroyEntity(m_Query);
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

            void Execute([EntityIndexInQuery] int idx, in Entity entity, in DamageData damage)
            {
                var context = new DefExt.WriterContext(Writer, idx);
                var damageConfig = damage.Bullet.Def.DamageType;
                foreach(var iter in damageConfig.Damages)
                {
                    if (damageConfig.Targets == DamageTargets.AoE)
                    {
                        AoE(idx, damageConfig.ID, damage.Target.WorldTransform.Position, iter, damage.Bullet.Range, damage.Value, context);
                    }
                    else
                    {
                        Damage(idx, damageConfig.ID, entity, iter, damage.Value, context);
                    }
                }
            }

            void Damage(int idx, ObjectID cfgID, Entity target, IDamage damage, float value, IDefineableContext context)
            {
                if (!LookupStats.HasBuffer(target)) return;
                var stats = LookupStats[target];

                damage.Apply(ref stats, value, context);
                Writer.AppendToBuffer(idx, target, new LastDamages() { DamageConfigID = cfgID, Value = value });
            }

            void AoE(int idx, ObjectID cfgID, float3 center, IDamage damage, float range, float value, IDefineableContext context)
            {
                using var targets = new NativeList<Entity>(Allocator.TempJob);
                FindEnemy(center, range, LookupTransforms, targets);

                foreach(var target in targets)
                {
                    Damage(idx, cfgID, target, damage, value, context);
                }
            }

            struct TempFindTarget
            {
                public Entity Entity;
                public float Magnitude;
                public WorldTransform Transform;
            }

            public void FindEnemy(float3 center, float radius, ComponentLookup<WorldTransform> transforms, NativeList<Entity> targets)
            {
                targets.Capacity = Entities.Length;
                var entities = Entities;
                var writer = targets.AsParallelWriter();

                Parallel.For(0, entities.Length, (i) =>
                {
                    var target = entities[i];
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
