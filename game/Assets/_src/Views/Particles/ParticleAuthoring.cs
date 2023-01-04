using System;
using Unity.Entities;
using UnityEngine;

namespace Game.Views
{
    public class ParticleAuthoring : MonoBehaviour
    {
        public string ID;
        class _baker : Baker<ParticleAuthoring>
        {
            public unsafe override void Bake(ParticleAuthoring authoring)
            {
                AddComponent<ParticleView>(authoring.ID);
            }
        }
    }
}