using Unity.Burst;
using Unity.Collections;
using Unity.Deformations;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace Game.Core
{
    internal partial struct Skin
    {
        [RequireMatchingQueriesForUpdate]
        [UpdateInGroup(typeof(PresentationSystemGroup))]
        [UpdateBefore(typeof(DeformationsInPresentation))]
        [BurstCompile]
        partial struct CalculateSkinMatrixSystemBase : ISystem
        {
            private EntityQuery m_Query;
            private ComponentLookup<LocalToWorld> m_LookupLocalToWorld;

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAllRW<SkinMatrix>()
                    .WithAll<Bone, Root>()
                    .Build();
                m_LookupLocalToWorld = state.GetComponentLookup<LocalToWorld>(true);
            }

            public void OnUpdate(ref SystemState state)
            {
                m_LookupLocalToWorld.Update(ref state);
                state.Dependency = new SystemJob()
                {
                    LookupLocalToWorld = m_LookupLocalToWorld,
                }.ScheduleParallel(state.Dependency);
            }
            
            [BurstCompile]
            private partial struct SystemJob : IJobEntity
            {
                [ReadOnly] public ComponentLookup<LocalToWorld> LookupLocalToWorld;

                [BurstCompile]
                public void Execute(ref DynamicBuffer<SkinMatrix> skinMatrices, in DynamicBuffer<Bone> bones, 
                    in Root rootEntityComponent)
                {
                    var root = rootEntityComponent.Value;
                    // Loop over each bone
                    for (int i = 0; i < skinMatrices.Length; ++i)
                    {
                        // Grab localToWorld matrix of bone
                        var bone = bones[i].Value;
                        var matrix = LookupLocalToWorld[bone].Value;

                        // Convert matrix relative to inverse root
                        var rootMatrixInv = math.inverse(LookupLocalToWorld[root].Value);
                        matrix = math.mul(rootMatrixInv, matrix);

                        // Compute to skin matrix
                        var bindPose = bones[i].BindPose;
                        matrix = math.mul(matrix, bindPose);

                        // Assign SkinMatrix
                        skinMatrices[i] = new SkinMatrix
                        {
                            Value = new float3x4(matrix.c0.xyz, matrix.c1.xyz, matrix.c2.xyz, matrix.c3.xyz)
                        };
                    }
                }
            }
        }
    }
}