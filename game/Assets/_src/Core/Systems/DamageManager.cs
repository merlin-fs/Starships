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
        ComponentLookup<Bullet> m_LookupBullets;
        EntityQuery m_QueryTargets;

        public struct NewDamages : IBufferElementData
        {
            public Entity Sender;
            public float Value;
        }

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<NewDamages>()
                .WithNone<DeadTag>()
                .Build();
            m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<NewDamages>());
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

        public static void Damage(Entity entity, Entity target, float value, IDefineableContext context)
        {
            var damage = new NewDamages
            {
                Sender = entity,
                Value = value,
            };
            context.AppendToBuffer(target, damage);
        }

        public unsafe void OnUpdate(ref SystemState state)
        {
            m_LookupTransforms.Update(ref state);
            m_LookupStats.Update(ref state);
            m_LookupBullets.Update(ref state);

            var entities = m_QueryTargets.ToEntityListAsync(Allocator.TempJob, state.Dependency, out JobHandle jobHandle);
            var system = state.World.GetExistingSystemManaged<GameLogicCommandBufferSystem>();
            var ecb = system.CreateCommandBuffer();

            var job = new WeaponJob()
            {
                Entities = entities,
                LookupStats = m_LookupStats,
                LookupTransforms = m_LookupTransforms,
                LookupBullets = m_LookupBullets,
                Writer = ecb.AsParallelWriter(),
            };
            state.Dependency = job.ScheduleParallel(m_Query, jobHandle);
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

            void Execute([EntityIndexInQuery] int idx, in Entity entity, ref DynamicBuffer<NewDamages> damages,
                ref DynamicBuffer<LastDamages> oldDamages)
            {
                oldDamages.Clear();
                var context = new DefExt.WriterContext(Writer, idx);
                foreach (var damage in damages)
                {
                    if (!LookupBullets.HasComponent(damage.Sender)) continue;
                    var bullet = LookupBullets[damage.Sender];
                    var damageConfig = bullet.Def.DamageType;

                    foreach(var iter in damageConfig.Damages)
                    {
                        if (damageConfig.Targets == DamageTargets.AoE)
                        {
                            AoE(idx, damageConfig.ID, damage.Sender, entity, iter, bullet.Range, damage.Value, ref oldDamages, context);
                        }
                        else
                        {
                            Damage(idx, damageConfig.ID, damage.Sender, entity, iter, damage.Value, ref oldDamages, context);
                        }
                    }
                }
                damages.Clear();
            }

            void Damage(int idx, ObjectID cfgID, Entity sender, Entity target, IDamage damage, float value, ref DynamicBuffer<LastDamages> damages, IDefineableContext context)
            {
                UnityEngine.Debug.Log($"[{target}], sender: {sender}, damage: {value}");

                if (!LookupStats.HasBuffer(target)) return;
                var stats = LookupStats[target];
                damage.Apply(ref stats, value, context);
                Writer.AppendToBuffer(idx, target, new LastDamages() { DamageConfigID = cfgID, Value = value });
            }

            void AoE(int idx, ObjectID cfgID, Entity sender, Entity center, IDamage damage, float range, float value, ref DynamicBuffer<LastDamages> damages, IDefineableContext context)
            {
                using var targets = new NativeList<Entity>(Allocator.TempJob);
                FindEnemy(center, range, LookupTransforms, targets);

                foreach(var target in targets)
                {
                    if (target != center) 
                        Damage(idx, cfgID, sender, target, damage, value, ref damages, context);
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
