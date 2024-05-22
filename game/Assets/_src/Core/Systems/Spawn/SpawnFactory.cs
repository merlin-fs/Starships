using System;
using System.Collections.Generic;
using System.Linq;

using Game.Model.Worlds;
using Game.Views;

using Reflex.Attributes;
using Reflex.Core;
using Reflex.Injectors;

using Unity.Entities;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Game.Core.Spawns
{
    public class SpawnFactory : MonoBehaviour, Spawn.IFactory
    {
        [SerializeField] private List<LayerItem> layerItems;

        private Dictionary<TypeIndex, Transform> m_Layers;
        
        private void Awake()
        {
            m_Layers = layerItems.ToDictionary(
                iter => TypeManager.GetTypeIndex(Type.GetType(iter.layer)),
                iter => iter.transform);
        }

        public IView Instantiate(GameObject prefab, Entity entity, Container container)
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var placement = manager.GetComponentData<Map.Placement>(entity);
            var parent = m_Layers[placement.Value.Layer];
            var obj = Object.Instantiate<GameObject>(prefab, parent);
            GameObjectInjector.InjectRecursive(obj, container);
            return obj.GetComponent<IView>();
        }

        [Serializable]
        private struct LayerItem
        {
            [SerializeField, SelectType(typeof(Map.Layers.ILayer))]
            public string layer;

            [SerializeField] public Transform transform;
        }
    }
}
