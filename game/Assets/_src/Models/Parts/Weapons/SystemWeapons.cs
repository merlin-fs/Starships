using Game.Model.Stats;
using Game.Model.Weapons;
using System;
using Unity.Collections;
using Unity.Entities;

namespace Game.Model
{
    [UpdateInGroup(typeof(GameLogicSystemGroup))]
    public partial class SystemWeapons : SystemBase
    {
        EntityQuery m_Query;
        EntityCommandBufferSystem m_EntityCommandBuffer;
                
        protected override void OnCreate()
        {
            m_EntityCommandBuffer = World.GetExistingSystemManaged<BeginSimulationEntityCommandBufferSystem>();
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Weapon>()
                .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities)
                .Build();
            RequireForUpdate(m_Query);
        }

        partial struct WeaponJob : IJobEntity
        {
            public uint LastSystemVersion;
            public float Delta;
            public EntityCommandBuffer.ParallelWriter Writer;
            
            void Execute([EntityIndexInQuery] int entityIndexInQuery, ref WeaponAspect weapon)
            {
                weapon.Time += Delta;
                if (weapon.Time >= weapon.Config.ReloadTime.Value)
                {
                    weapon.Reload(Writer, entityIndexInQuery);
                    weapon.Time = 0;
                }
            }
        }


        protected override void OnUpdate()
        {
            var ecb = m_EntityCommandBuffer.CreateCommandBuffer();
            var job = new WeaponJob()
            {
                Writer = ecb.AsParallelWriter(),
                LastSystemVersion = LastSystemVersion,
                Delta = SystemAPI.Time.DeltaTime,
            };
            var handle = job.ScheduleParallel(m_Query, Dependency);
            handle.Complete();
            //Dependency = job.ScheduleParallel(m_Query, Dependency);
            //m_EntityCommandBuffer.AddJobHandleForProducer(Dependency);
        }
    }
}
