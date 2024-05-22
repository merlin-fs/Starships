using System;
using System.Collections.Generic;
using System.Linq;

using Common.Core;

using Game.Core;
using Game.Model.Logics;

using Reflex.Attributes;

using Unity.Collections;

using UnityEngine;
using UnityEngine.AddressableAssets;
using Hash128 = Unity.Entities.Hash128;

namespace Game.Views
{
    public class ViewVfxComponent : MonoBehaviour, IViewLogicComponent
    {
        [Inject] private RepositoryVfx m_RepositoryVfx;
        [Inject] private ParticleManager m_ParticleManager;
        
        [SerializeField] 
        private List<StateItem> m_StateItems;

        private Map<LogicActionHandle, VfxValue> m_States = new(5, Allocator.Persistent, true);

        public void ChangeAction(LogicActionHandle action)
        {
            if (!m_States.TryGetValues(action, out var values)) return;

            foreach (var iter in values)
            {
                if (!m_RepositoryVfx.TryGet(iter.VfxID, out var vfx)) return;

                m_ParticleManager.Play(vfx, target =>
                {
                    var self = gameObject.FindObjectFromPath(iter.TargetPath.ToString());
                    if (iter.Position)
                        target.position = self.transform.position;
                    if (iter.Scale)
                        target.localScale = self.transform.lossyScale;
                    if (iter.Rotation)
                        target.localRotation = self.transform.rotation;
                });
            }
        }

        private void Awake()
        {
            foreach (var iter in m_StateItems)
            {
                var id = new Hash128(iter.Vfx.AssetGUID);
                m_States.Add(
                    LogicActionHandle.FromType(Type.GetType(iter.State)),
                    new VfxValue 
                    {
                        VfxID = id,
                        TargetPath = iter.Target.gameObject.GetHierarchyPath(gameObject),
                        Position = iter.Position, 
                        Rotation = iter.Rotation, 
                        Scale = iter.Scale,
                        ScaleTime = iter.ScaleTime,
                    });
            }
        }

        private void Start()
        {
            foreach (var id in m_StateItems.Select(iter => new Hash128(iter.Vfx.AssetGUID)))
            {
                m_RepositoryVfx.CachedVfx(id);
            }
        }

        private struct VfxValue
        {
            public Hash128 VfxID;
            public FixedString64Bytes TargetPath;
            public bool Scale;
            public bool Position;
            public bool Rotation;
            public float ScaleTime;
        }

        [Serializable]
        private class StateItem
        {
            [SerializeField, SelectType(typeof(Logic.IAction))]
            public string State;
            public AssetReferenceT<GameObject> Vfx;
            public Transform Target;
            public bool Position = true;
            public bool Rotation = true;
            public bool Scale = true;
            public float ScaleTime = 1;
        }
    }
}
