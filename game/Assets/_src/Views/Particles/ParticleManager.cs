using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using Common.Singletons;

namespace Game.Views
{
    public class ParticleManager: MonoSingleton<ParticleManager>
    {
        public static ParticleManager Instance => Inst;

        [SerializeField]
        private int m_DefaultCapacity = 10;
        [SerializeField]
        private int m_MaxPoolSize = 15;

        private Dictionary<Unity.Entities.Hash128, ObjectsPoolAsync<PooledObject>> m_Pool = new Dictionary<Unity.Entities.Hash128, ObjectsPoolAsync<PooledObject>>();

        private class PooledObject
        {
            public GameObject Paricle;
            public Unity.Entities.Hash128 VfxID;
        }

        public async void Play(ParticleTrigger particleTrigger, LocalToWorld transform)
        {
            PooledObject obj = await Get(particleTrigger);
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

        protected override void Awake()
        {
            base.Awake();
        }


        private void Release(PooledObject obj)
        {
            if (m_Pool.TryGetValue(obj.VfxID, out ObjectsPoolAsync<PooledObject> pool))
                pool.Release(obj);
        }

        private async Task<PooledObject> Get(ParticleTrigger particleTrigger)
        {
            if (!m_Pool.TryGetValue(particleTrigger.VfxID, out ObjectsPoolAsync<PooledObject> pool))
            {
                pool = new ObjectsPoolAsync<PooledObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool,
                OnDestroyPoolObject, true, m_DefaultCapacity, m_MaxPoolSize);
                m_Pool.Add(particleTrigger.VfxID, pool);
            }
            return await pool.Get(particleTrigger);
        }

        private async Task<PooledObject> CreatePooledItem(object arg)
        {
            var particle = (ParticleTrigger)arg;
            var reference = new AssetReferenceT<GameObject>(particle.VfxID.ToString());
            var prefab = !reference.IsValid()
                ? await reference.LoadAssetAsync().Task
                : (GameObject)reference.Asset;

            var obj = GameObject.Instantiate(prefab, transform);
            obj.AddComponent<ParticleEvent>();

            return new PooledObject{ Paricle = obj, VfxID= particle.VfxID };
        }

        private void OnTakeFromPool(PooledObject obj) => obj.Paricle.SetActive(true);

        private void OnReturnedToPool(PooledObject obj) => obj.Paricle.SetActive(false);

        private void OnDestroyPoolObject(PooledObject obj) => Destroy(obj.Paricle);
    }
}