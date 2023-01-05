using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Entities;

namespace Game.Views.Stats
{
    using Model.Stats;
    
    public struct StatView: IBufferElementData, IDisposable
    {
        private IntPtr m_ViewHandle;
        public int StatID;

        public IStatViewComponent View => (IStatViewComponent)GCHandle.FromIntPtr(m_ViewHandle).Target;

        public StatView(IStatViewComponent value, int statId)
        {
            StatID = statId;
            unsafe 
            {
                UnsafeUtility.PinGCObjectAndGetAddress(value, out ulong handle);
                m_ViewHandle = new IntPtr((long)handle);
            }
        }

        public void Dispose()
        {
            unsafe
            {
                UnsafeUtility.ReleaseGCObject((ulong)m_ViewHandle.ToInt64());
            }
        }
    }

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

        void IStatViewComponent.Update(Stat stat, ITransformData transform)
        {
            m_Position = transform._Position;
            m_Initialize = true;
            m_Value = stat.Normalize;
        }

        public void SetDestroy()
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
}