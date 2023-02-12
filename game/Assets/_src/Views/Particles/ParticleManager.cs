using System;
using Unity.Entities;
using Unity.Transforms;
using Common.Singletons;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Views
{
    public class ParticleManager: MonoSingleton<ParticleManager>
    {
        public static ParticleManager Instance => Inst;
        public async void Play(Entity entity, Particle particle, WorldTransform worldTransform)
        {
            var reference = new AssetReferenceT<GameObject>(particle.VfxID.ToString());

            GameObject prefab = !reference.IsValid()
                ? await reference.LoadAssetAsync().Task
                : (GameObject)reference.Asset;
            var inst = Instantiate(prefab, worldTransform.Position, Quaternion.identity, transform);//worldTransform.Rotation
            inst.transform.localScale = new Vector3(worldTransform.Scale, worldTransform.Scale, worldTransform.Scale);

            inst.GetComponent<ParticleSystem>().Play(true);
        }
    }
}