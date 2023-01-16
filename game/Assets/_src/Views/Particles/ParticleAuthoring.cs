using System;

using Game.Model.Stats;
using Game.Model.Weapons;

using Unity.Entities;
using UnityEngine;

namespace Game.Views
{
    public class ParticleAuthoring : MonoBehaviour
    {
#if UNITY_EDITOR
        [SelectChildPrefab]
        public GameObject Target;
#endif
        /*
        class _baker : Baker<ParticleAuthoring>
        {
            public unsafe override void Bake(ParticleAuthoring authoring)
            {
                AppendToBuffer(new Particle
                {
                    StateID = Stat.GetID(Weapon.State.Shoot),
                    Target = GetEntity(authoring.Target),
                });
            }
        }
        */
    }
}