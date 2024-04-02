using Unity.Entities;
using UnityEngine;

namespace Game.Core
{
    internal partial struct Skin
    {
        internal class DeformationBaker : Baker<SkinnedMeshRenderer>
        {
            public override void Bake(SkinnedMeshRenderer authoring)
            {
                var skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>(authoring);
                if (skinnedMeshRenderer == null)
                    return;

                if (skinnedMeshRenderer.sharedMesh == null)
                    return;

                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Only execute this if we have a valid skinning setup
                DependsOn(skinnedMeshRenderer.sharedMesh);
                var hasSkinning = skinnedMeshRenderer.bones.Length > 0 &&
                                  skinnedMeshRenderer.sharedMesh.bindposes.Length > 0;
                if (hasSkinning)
                {
                    // Setup reference to the root bone
                    var rootTransform = skinnedMeshRenderer.rootBone
                        ? skinnedMeshRenderer.rootBone
                        : skinnedMeshRenderer.transform;
                    var rootEntity = GetEntity(rootTransform, TransformUsageFlags.Dynamic);
                    AddComponent(entity, new Root {Value = rootEntity});

                    // Setup reference to the other bones
                    var boneEntityArray = AddBuffer<Bone>(entity);
                    boneEntityArray.ResizeUninitialized(skinnedMeshRenderer.bones.Length);

                    for (int boneIndex = 0; boneIndex < skinnedMeshRenderer.bones.Length; ++boneIndex)
                    {
                        var bone = skinnedMeshRenderer.bones[boneIndex];
                        var boneEntity = GetEntity(bone, TransformUsageFlags.Dynamic);
                        var bindPose = skinnedMeshRenderer.sharedMesh.bindposes[boneIndex];
                        boneEntityArray[boneIndex] = new Bone { Value = boneEntity, BindPose = bindPose};
                    }
                }
            }
        }
    }
}
