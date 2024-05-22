using System;

using Game.Core;
using Game.Model.Logics;
using Game.Model.Weapons;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Views
{
#if UNITY_EDITOR
    public class ParticleAuthoring : MonoBehaviour
    {
        [SelectChildPrefab]
        public GameObject Target;
        public AssetReferenceT<GameObject> Vfx;
        public bool Position = true;
        public bool Rotation = true;
        public bool Scale = true;
        public float ScaleTime = 1;

        class _baker : Baker<ParticleAuthoring>
        {
            public unsafe override void Bake(ParticleAuthoring authoring)
            {
/*
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var buffer = AddBuffer<ParticleTrigger>(entity);

                buffer.Add(new ParticleTrigger
                {
                    Action = EnumHandle.FromEnum(Weapon.Action.Shoot),
                    VfxID = new Hash128(authoring.Vfx.AssetGUID),
                    Target = GetEntity(authoring.Target, TransformUsageFlags.Dynamic),
                    Position = authoring.Position, 
                    Rotation = authoring.Rotation, 
                    Scale = authoring.Scale,
                    ScaleTime = authoring.ScaleTime,
                });
                */
            }
        }
    }
#endif
}