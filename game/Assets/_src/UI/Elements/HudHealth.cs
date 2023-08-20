using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using Common.Core;

namespace Game.UI.Huds
{
    public abstract class Hud : IHud
    {
        protected HudManager HudManager { get; private set; }
        protected VisualElement Element { get; private set; }
        public abstract void SetPosition(float3 position);
        public virtual void Initialize(HudManager hudManager, VisualElement element)
        {
            HudManager = hudManager;
            Element = element;
        }
    }

    public class HudHealth : Hud
    {
        private float3 m_Position;
        private ProgressBar m_Progress;

        public override void Initialize(HudManager hudManager, VisualElement element)
        {
            base.Initialize(hudManager, element);
            m_Progress = element.Q<ProgressBar>("progress");
            m_Progress.style.position = Position.Absolute;
        }
        public override void SetPosition(float3 position)
        {
            float scale = 1 + HudManager.WorldCamera.transform.localPosition.z / 200;
            position.y += 1.1f; 
            Vector2 newPosition = RuntimePanelUtils.CameraTransformWorldToPanel(m_Progress.panel, position, HudManager.WorldCamera);
            
            
            newPosition.x = (newPosition.x - m_Progress.layout.width / 2);
            m_Progress.transform.scale = new Vector3(scale, scale, scale);
            //newPosition.y *= scale;
            m_Progress.transform.position = newPosition;
        }

        public IVisualElementScheduledItem UpdatePositionSchedule()
        {
            return m_Progress.schedule.Execute(() => SetPosition(m_Position));
        } 
            
        public void UpdatePositionSchedule(float3 position)
        {
            m_Position = position;
        }
    }
}
