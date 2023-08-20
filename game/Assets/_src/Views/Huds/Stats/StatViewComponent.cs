using System;
using System.Threading;
using Unity.Transforms;
using Unity.Entities;

using Common.Core;
using Common.Defs;
using Game.Model.Stats;
using Game.UI.Huds;
using Game.Core;

using JetBrains.Annotations;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Views.Stats
{
    public struct StatView : IComponentData, IStatViewComponent
    {
        private readonly DiContext.Var<HudManager> m_HudManager;
        private RefLink<HudHealth> m_Element;
        private HudHealth Element => m_Element.Value;
        private EnumHandle m_StatID;

        public StatView(EnumHandle statID)
        {
            m_StatID = statID;
            m_Element = RefLink<HudHealth>.From(m_HudManager.Value.GetHud<HudHealth>());
            Element.UpdatePositionSchedule()
                .When(() => Time.frameCount % 3 == 0)
                .EveryFrame();
        }
        public void Update(in StatAspect stat, in LocalTransform transform)
        {
            Element.UpdatePositionSchedule(transform.Position);
        }
        
        public void Dispose()
        {
            m_HudManager.Value.ReleaseHud(Element);
            RefLink<HudHealth>.Free(m_Element);
        }
    }

    /*
    public class StatViewComponent : MonoBehaviour, IStatViewComponent
    {
        [SerializeField]
        private GameObject m_Root;
        [SerializeField]
        private Image m_Progress;

        private float3 m_Position;
        private float m_Value;
        private Canvas m_Canvas;
        private bool m_Destroing = false;
        private bool m_Initialize = false;

        private void Awake()
        {
            m_Root.SetActive(false);
        }

        void Start()
        {
            m_Canvas = GetComponentInParent<Canvas>();
        }

        void IStatViewComponent.Update(Stat stat, LocalTransform transform)
        {
            m_Position = transform.Position;
            m_Initialize = true;
            m_Value = stat.Normalize;
        }

        public void Dispose()
        {
            m_Destroing = true;
        }

        private void Update()
        {
            if (m_Destroing)
            {
                GameObject.Destroy(gameObject);
                return;
            }
            if (m_Initialize && !m_Root.activeSelf)
                m_Root.SetActive(true);

            float3 value = Camera.main.WorldToScreenPoint(m_Position);
            value.z = transform.position.z;
            value *= m_Canvas.transform.localScale;
            transform.position = value;
            m_Progress.fillAmount = m_Value;
        }
    }
    */
}