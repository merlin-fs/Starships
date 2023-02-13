using System;
using Unity.Entities;
using Unity.Transforms;
using Common.Singletons;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using System.Threading.Tasks;

namespace Game.Views
{
    public class ParticleManager: MonoSingleton<ParticleManager>
    {
        public static ParticleManager Instance => Inst;

        [SerializeField]
        private int m_DefaultCapacity = 10;
        [SerializeField]
        private int m_MaxPoolSize = 15;

        private PoolObjects<GameObject, Particle> m_Pool;

        public async void Play(Entity entity, Particle particle, WorldTransform worldTransform)
        {
            GameObject obj = await m_Pool.Get(particle);
            obj.transform.position = worldTransform.Position;
            //obj.transform.localRotation = worldTransform.Rotation
            //obj.transform.localScale = new Vector3(worldTransform.Scale, worldTransform.Scale, worldTransform.Scale);
            obj.SetActive(true);
            var system = obj.GetComponent<ParticleSystem>();

            obj.GetComponent<ParticleEvent>().OnStop += (inst) => 
            {
                m_Pool.Release(inst.gameObject);
            };
            obj.GetComponent<ParticleSystem>().Play(true);
        }

        protected override void Awake()
        {
            base.Awake();
            m_Pool = new PoolObjects<GameObject, Particle>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool,
                OnDestroyPoolObject, true, m_DefaultCapacity, m_MaxPoolSize);
        }

        private async Task<GameObject> CreatePooledItem(Particle arg)
        {
            var reference = new AssetReferenceT<GameObject>(arg.VfxID.ToString());
            var prefab = !reference.IsValid()
                ? await reference.LoadAssetAsync().Task
                : (GameObject)reference.Asset;

            var obj = GameObject.Instantiate(prefab, transform);
            obj.AddComponent<ParticleEvent>();
            return obj;
        }

        private void OnTakeFromPool(GameObject obj) => obj.SetActive(true);

        private void OnReturnedToPool(GameObject obj) => obj.SetActive(false);

        private void OnDestroyPoolObject(GameObject obj) => Destroy(obj);
    }
}