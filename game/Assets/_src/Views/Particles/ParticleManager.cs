using System;
using System.Collections.Generic;
using UnityEngine;
using System.Pool;
using Hash128 = Unity.Entities.Hash128;

namespace Game.Views
{
    public class ParticleManager : MonoBehaviour
    {
        [SerializeField] private int maxPoolSize = 1000;
        private readonly Dictionary<Hash128, ObjectPool<PooledObject>> m_Pool = new();

        private class PooledObject
        {
            public ParticleSystem Paricle;
            public Hash128 ID;
        }
        public void Play(IItemVfx vfxItem, Action<Transform> onPlace)
        {
            PooledObject obj = Get(vfxItem, onPlace);
            var system = obj.Paricle.GetComponent<ParticleSystem>();
            system.Play(true);
        }

        private void Release(PooledObject obj)
        {
            if (m_Pool.TryGetValue(obj.ID, out ObjectPool<PooledObject> pool))
                pool.Release(obj);
        }

        private PooledObject Get(IItemVfx vfxItem, Action<Transform> onPlace)
        {
            if (m_Pool.TryGetValue(vfxItem.ID, out ObjectPool<PooledObject> pool)) 
                return pool.Get(vfxItem);
            
            pool = new ObjectPool<PooledObject>(
                arg => CreatePooledItem((IItemVfx)arg),
                (obj, _) =>
                {
                    onPlace?.Invoke(obj.Paricle.transform);
                    obj.Paricle.gameObject.SetActive(true);
                    obj.Paricle.gameObject.AddComponent<ParticleEvent>()
                        .OnStop += _ => Release(obj); 
                },
                obj =>
                {
                    obj.Paricle.gameObject.SetActive(false);
                },
                obj => Destroy(obj.Paricle.gameObject),
                maxPoolSize);
            m_Pool.Add(vfxItem.ID, pool);
            return pool.Get(vfxItem);
        }

        private PooledObject CreatePooledItem(IItemVfx vfxItem)
        {
            return new PooledObject 
            {
                Paricle = Instantiate(vfxItem.ParticleSystem, transform), 
                ID = vfxItem.ID
            };
        }
    }
}