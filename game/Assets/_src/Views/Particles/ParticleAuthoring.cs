using System;
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

        class _baker : Baker<ParticleAuthoring>
        {
            public unsafe override void Bake(ParticleAuthoring authoring)
            {
                var buffer = AddBuffer<Particle>();
                buffer.Add(new Particle
                {
                    Action = LogicHandle.FromEnum(Weapon.Action.Shoot),
                    VfxID = new Unity.Entities.Hash128(authoring.Vfx.AssetGUID),
                    Target = GetEntity(authoring.Target),
                    Position = authoring.Position, 
                    Rotation = authoring.Rotation, 
                    Scale = authoring.Scale,
                });
            }
        }
    }
#endif
}