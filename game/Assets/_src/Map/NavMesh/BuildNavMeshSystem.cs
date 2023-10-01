using System.Collections.Generic;
using System.Threading;

using Buildings.Environments;

using Game.Core.Spawns;

using Unity.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.AI;

namespace Game.Model.Worlds
{
    public partial struct Map
    {
        [UpdateInGroup(typeof(GameEndSystemGroup))]
        //[UpdateAfter(typeof(MoveSystem))]
        public partial struct BuildNavMeshSystem : ISystem
        {
            private EntityQuery m_Query;
            private EntityQuery m_QueryMap;
            
            private static int OBSTACLE = NavMesh.GetAreaFromName("Not Walkable");

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAll<NavMeshSourceData, Map.Placement, LocalTransform, LocalToWorld>()
                    .WithAny<SelectBuildingTag, Spawn.Tag>()
                    .Build();

                m_QueryMap = SystemAPI.QueryBuilder()
                    .WithAll<NavMeshBuildTag>()
                    //.WithNone<SelectBuildingTag, Spawn.Tag>()
                    .Build();
                state.RequireForUpdate(m_QueryMap);
            }

            public void OnUpdate(ref SystemState state)
            {
                if (m_Query.IsEmpty) return;

                var system = SystemAPI.GetSingleton<GameSpawnSystemCommandBufferSystem.Singleton>();
                var ecb = system.CreateCommandBuffer(state.WorldUnmanaged);
                var aspect = SystemAPI.GetAspect<Map.Aspect>(m_QueryMap.GetSingletonEntity());

                var sources = new NativeArray<NavMeshBuildSource>(m_Query.CalculateEntityCount(), Allocator.TempJob);
                state.Dependency = new BuildSourcesJob 
                {
                    Aspect = aspect,
                    Sources = sources
                }.ScheduleParallel(m_Query, state.Dependency);
                
                state.Dependency = new SystemJob
                {
                    Sources = sources,
                }.Schedule(state.Dependency);
                ecb.RemoveComponent<NavMeshBuildTag>(m_QueryMap, EntityQueryCaptureMode.AtPlayback);
                state.Dependency = sources.Dispose(state.Dependency);
            }

            partial struct BuildSourcesJob : IJobEntity
            {
                public NativeArray<NavMeshBuildSource> Sources;
                [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
                public Map.Aspect Aspect;
                    
                private void Execute([EntityIndexInQuery] int idx, in NavMeshSourceData data, 
                    LocalTransform lt)
                {
                    var position = lt.Position + math.mul(lt.Rotation, data.Transform.c3.xyz);
                    Sources[idx] = new NavMeshBuildSource 
                    {
                        shape = data.Shape,
                        size = data.Size,
                        transform = float4x4.TRS(position, lt.Rotation, lt.Scale),
                        area =  OBSTACLE,
                    };
                }
            }
            
            struct SystemJob : IJob
            {
                public NativeArray<NavMeshBuildSource> Sources;
                
                public unsafe void Execute()
                {
                    var list = new List<NavMeshBuildSource>();
                    list.AddRange(Sources);
                    
                    UnityMainThread.Context.Post(obj =>
                    {
                        var sources = (List<NavMeshBuildSource>)list;
                        /*
                        var navMeshData = new NavMeshData(0);
                        NavMesh.AddNavMeshData(navMeshData);
                        var navMeshBuildSettings = NavMesh.GetSettingsByID(0);
                        navMeshBuildSettings.debug = new NavMeshBuildDebugSettings {flags = NavMeshBuildDebugFlags.All};
                        //list.AddRangeNative(Sources.GetUnsafeReadOnlyPtr(), Sources.Length);
                        var success = NavMeshBuilder.UpdateNavMeshData(navMeshData, navMeshBuildSettings, sources, new Bounds( new Vector3(10000, 10000, 10000), Vector3.zero));
                        */
                        var navMeshBuildSettings = NavMesh.GetSettingsByID(0);
                        //var navMeshBuildSettings = NavMesh.CreateSettings();
                        navMeshBuildSettings.minRegionArea = 10;
                        navMeshBuildSettings.voxelSize = 0.1f;
                        navMeshBuildSettings.tileSize = 128;
                        navMeshBuildSettings.debug = new NavMeshBuildDebugSettings {flags = NavMeshBuildDebugFlags.All};

                        var floor = new NavMeshBuildSource
                        {
                            transform = float4x4.TRS(new float3(0, -0.5f, 0), quaternion.identity, 1),
                            shape = NavMeshBuildSourceShape.Box,
                            size = new Vector3(1000, 1, 1000)
                        };
                        sources.Add(floor);
    
                        /*
                        // Create obstacle 
                        const int OBSTACLE = 1 << 0;
                        var obstacle = new NavMeshBuildSource
                        {
                            transform = Matrix4x4.TRS(new Vector3(3,0,3), quaternion.identity, Vector3.one),
                            shape = NavMeshBuildSourceShape.Box,
                            size = new Vector3(1, 1, 1),
                            area = OBSTACLE
                        }; 
                        sources.Add(obstacle);
                        */
    
                        // build navmesh
                        NavMeshData built = NavMeshBuilder.BuildNavMeshData(
                            navMeshBuildSettings, sources, new Bounds(Vector3.zero, new Vector3(100,100,100)), 
                            new Vector3(0,-1.5f,0), quaternion.identity);
                        NavMesh.AddNavMeshData(built);
                        //Assert.IsTrue(success);
                        
                    }, list);
                }
            }
        }
    }
}