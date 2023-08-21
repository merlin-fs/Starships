using System;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Huds
{
    public class HudHealth : Hud
    {
        private float3 m_Position;
        private float m_Value;
        private ProgressBar m_Progress;

        protected override void Configure(VisualElement element)
        {
            base.Configure(element);
            m_Progress = element.Q<ProgressBar>("progress");
            m_Progress.highValue = 1f;
            m_Progress.style.position = Position.Absolute;
            m_Progress.schedule.Execute(() =>
            {
                m_Progress.value = m_Value;
                SetPosition(m_Position, m_Progress);
            }).When(() => Time.frameCount % 3 == 0).EveryFrame();
        }

        public void Update(float3 position, float value)
        {
            m_Value = value;
            m_Position = position;
        }
    }
}
