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
        public AssetReferenceT<GameObject> Vfx;

        class _baker : Baker<ParticleAuthoring>
        {
            public unsafe override void Bake(ParticleAuthoring authoring)
            {
                var buffer = AddBuffer<Particle>();
                buffer.Add(new Particle
                {
                    Action = LogicHandle.FromEnum(Weapon.Action.Shoot),
                    VfxID = new Unity.Entities.Hash128(authoring.Vfx.AssetGUID),
                });
            }
        }
    }
#endif
}