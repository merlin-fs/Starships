using System;
using System.Collections.Generic;
using Unity.Transforms;
using UnityEngine;
using System.Pool;

using Game.Core;

using Reflex.Attributes;

namespace Game.Views
{
    public class ParticleManager : MonoBehaviour
    {
        [Inject] private RepositoryVfx m_LibraryVfx;
        [SerializeField] private int maxPoolSize = 15;

        private readonly Dictionary<Unity.Entities.Hash128, ObjectPool<PooledObject>> m_Pool = new();

        private class PooledObject
        {
            public GameObject Paricle;
            public Unity.Entities.Hash128 VfxID;
        }
/*
        public void Play(id Action<Transform> onPlace)
        {
            PooledObject obj = Get(particleTrigger);
            onPlace?.Invoke();
            
            if (particleTrigger.Position)
                obj.Paricle.transform.position = transform.Position;
            if (particleTrigger.Scale)
                obj.Paricle.transform.localScale = transform.Scale();
            if (particleTrigger.Rotation)
                obj.Paricle.transform.localRotation = transform.Rotation;
            
            obj.Paricle.SetActive(true);
            var system = obj.Paricle.GetComponent<UnityEngine.ParticleSystem>();
            obj.Paricle.GetComponent<ParticleEvent>().OnStop += (inst) => 
            {
                Release(obj);
            };
            
            foreach (var iter in system.GetComponentsInChildren<UnityEngine.ParticleSystem>(true))
            {
                var main = iter.main;
                main.simulationSpeed = particleTrigger.ScaleTime;
            }
            system.Play(true);
        }
        */

        private void Release(PooledObject obj)
        {
            if (m_Pool.TryGetValue(obj.VfxID, out ObjectPool<PooledObject> pool))
                pool.Release(obj);
        }

        private PooledObject Get(ParticleTrigger particleTrigger)
        {
            if (!m_Pool.TryGetValue(particleTrigger.VfxID, out ObjectPool<PooledObject> pool))
            {
                pool = new ObjectPool<PooledObject>(
                    CreatePooledItem, 
                    (obj, _) => obj.Paricle.SetActive(true),
                    obj => obj.Paricle.SetActive(false),
                    obj => Destroy(obj.Paricle),
                    maxPoolSize);
                m_Pool.Add(particleTrigger.VfxID, pool);
            }
            return pool.Get(particleTrigger);
        }

        private PooledObject CreatePooledItem(object arg)
        {
            return null;
            /*
            var particle = (ParticleTrigger)arg;
            var prefab = m_LibraryVfx.GetVfx(particle.VfxID);
            var obj = GameObject.Instantiate(prefab, transform);
            obj.AddComponent<ParticleEvent>();
            return new PooledObject{ Paricle = obj, VfxID = particle.VfxID };
            */
        }
    }
}