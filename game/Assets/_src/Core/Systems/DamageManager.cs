using System;
using Unity.Entities;

namespace Game.Model.Weapons
{
    using Stats;
    using Logics;

    [DisableAutoCreation]
    [UpdateInGroup(typeof(GameLogicDoneSystemGroup))]
    [UpdateAfter(typeof(BatleManager))]
    public partial struct DamageManager : ISystem
    {
        EntityQuery m_Query;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Stat>()
                .WithAll<Damage>()
                .WithNone<DeadTag>()
                .Build();
            state.RequireForUpdate(m_Query);
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var system = state.World.GetExistingSystemManaged<GameLogicCommandBufferSystem>();
            var ecb = system.CreateCommandBuffer();
            var job = new WeaponJob()
            {
                Writer = ecb.AsParallelWriter(),
                Delta = SystemAPI.Time.DeltaTime,
            };
            state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
            state.CompleteDependency();
        }

        partial struct WeaponJob : IJobEntity
        {
            public float Delta;
            public EntityCommandBuffer.ParallelWriter Writer;
            void Execute([EntityIndexInQuery] int idx, in Entity entity, in Damage damage, ref StatAspect stats)
            {
                stats.Values.GetRW(GlobalStat.Health).Damage(damage.Value);
                Writer.RemoveComponent<Damage>(idx, entity);
            }
        }
    }
}
