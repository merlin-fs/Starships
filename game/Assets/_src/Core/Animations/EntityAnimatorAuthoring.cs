using Unity.Burst;
using Unity.Collections;

using UnityEngine;
using Unity.Entities;

namespace Game.Core.Animations
{
    public class EntityAnimatorAuthoring : MonoBehaviour
    {
        public class _baker : Baker<EntityAnimatorAuthoring>
        {
            public override void Bake(EntityAnimatorAuthoring authoring)
            {
                var root = GetEntity(TransformUsageFlags.Dynamic);
                //AddComponent(root, new Animation());
                //AddComponent(root, new Animation.CurrentClip());
                AddComponent(root, new Animation.NextClip());
                AddBuffer<AnimationBakingBone>(root);
                
                AddComponent(root, new Animation 
                {
                    Playing = true,
                    InTransition = false,
                    SpeedMultiplier = 1f,
                });

                AddComponent(root, new Animation.CurrentClip 
                {
                    Data = new Animation.ClipData 
                    {
                        ClipID = "New Animation",
                        Elapsed = 0,
                        Duration = 1f,
                        Speed = 1f,
                        Loop = true,
                    }
                });

                foreach (var iter in authoring.transform.GetComponentsInChildren<Transform>())
                {
                    var bone = GetEntity(iter, TransformUsageFlags.Dynamic);
                    var name = (root == bone)
                        ? EntityAnimatorConfig.ROOT_NAME
                        : iter.name;

                    this.AppendToBuffer(root, new AnimationBakingBone 
                    {
                        Entity = bone,
                        BoneID = UnityEngine.Animator.StringToHash(name), 
                    });
                }
            }
        }
    }
    
    [TemporaryBakingType]
    internal struct AnimationBakingBone: IBufferElementData
    {
        public Entity Entity;
        public int BoneID;
    }
    
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial struct AnimationBakingSystem : ISystem
    {
        private EntityQuery m_Query;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<AnimationBakingBone>()
                .WithOptions(EntityQueryOptions.IncludeDisabledEntities | EntityQueryOptions.IncludePrefab)
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var job = new SystemJob
            {
                Writer = ecb.AsParallelWriter(),
            }.ScheduleParallel(m_Query, state.Dependency);
            job.Complete();
            ecb.RemoveComponent<AnimationBakingBone>(m_Query, EntityQueryCaptureMode.AtPlayback);

            ecb.Playback(state.EntityManager);
        }

        [BurstCompile]
        private partial struct SystemJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;

            [BurstCompile]
            private void Execute([EntityIndexInQuery] int idx, in Entity entity, 
                in DynamicBuffer<AnimationBakingBone> buffer)
            {
                foreach (var iter in buffer)
                {
                    Writer.AddComponent(idx, iter.Entity, new Animation.Bone 
                    {
                        Animator = entity,
                        BoneID = iter.BoneID,
                    });
                }
            }
        }
    }
    
}