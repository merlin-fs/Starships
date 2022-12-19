using System;
using Unity.Entities;
using Common.Defs;
using Game.Model.Stats;
using static UnityEngine.EventSystems.EventTrigger;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Game.Model.Weapons
{

    [UpdateInGroup(typeof(GameLogicSystemGroup))]
    public partial struct WeaponSystem : ISystem
    {
        EntityQuery m_Query;
               
        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Weapon>()
                .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities)
                .Build();

            state.RequireForUpdate(m_Query);
        }

        public void OnDestroy(ref SystemState state)
        {

        }

        partial struct WeaponJob : IJobEntity
        {
            [NativeDisableContainerSafetyRestriction]
            public EntityManager Manager;
            public uint LastSystemVersion;
            public float Delta;
            public EntityCommandBuffer.ParallelWriter Writer;

            void Execute(in Entity entity, [EntityIndexInQuery] int entityIndexInQuery, ref WeaponAspect weapon)
            {
                weapon.Time += Delta;
                if (weapon.Time >= weapon.Config.ReloadTime.Value)
                {
                    //weapon.Reload(new DefExt.EntityManagerContext(Manager));//, entityIndexInQuery
                    weapon.Reload(new DefExt.WriterContext(Writer, entityIndexInQuery));
                    weapon.Time = 0;
                }
            }
        }


        public void OnUpdate(ref SystemState state)
        {
            var ecb = state.World.GetExistingSystemManaged<GameLogicCommandBufferSystem>().CreateCommandBuffer();
            var job = new WeaponJob()
            {
                Manager = state.EntityManager,
                Writer = ecb.AsParallelWriter(),
                LastSystemVersion = state.LastSystemVersion,
                Delta = SystemAPI.Time.DeltaTime,
            };
            state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
            state.Dependency.Complete();
        }
    }
}
