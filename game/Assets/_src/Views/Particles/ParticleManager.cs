using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Entities;
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

        public async void Play(Entity entity, Particle particle, WorldTransform worldTransform)
        {
            PooledObject obj = await Get(particle);
            if (particle.Position)
                obj.Paricle.transform.position = worldTransform.Position;
            if (particle.Scale)
                obj.Paricle.transform.localScale = new Vector3(worldTransform.Scale, worldTransform.Scale, worldTransform.Scale);
            if (particle.Rotation)
                obj.Paricle.transform.localRotation = worldTransform.Rotation;
            
            obj.Paricle.SetActive(true);
            var system = obj.Paricle.GetComponent<UnityEngine.ParticleSystem>();
            obj.Paricle.GetComponent<ParticleEvent>().OnStop += (inst) => 
            {
                Release(obj);
            };
            obj.Paricle.GetComponent<UnityEngine.ParticleSystem>().Play(true);
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

        private async Task<PooledObject> Get(Particle particle)
        {
            if (!m_Pool.TryGetValue(particle.VfxID, out ObjectsPoolAsync<PooledObject> pool))
            {
                pool = new ObjectsPoolAsync<PooledObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool,
                OnDestroyPoolObject, true, m_DefaultCapacity, m_MaxPoolSize);
                m_Pool.Add(particle.VfxID, pool);
            }
            return await pool.Get(particle);
        }

        private async Task<PooledObject> CreatePooledItem(object arg)
        {
            var particle = (Particle)arg;
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