using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Elements
{
    public class FpsLabel : Label
    {
        public new class UxmlFactory : UxmlFactory<FpsLabel, UxmlTraits> { }

        const string display = "{0} FPS";
        const int m_FpsRange = 50;
        readonly int[] m_FpsBuffer = new int[m_FpsRange];
        int m_FpsBufferIndex;
        float m_AverageFPS;

        public new class UxmlTraits : Label.UxmlTraits
        {
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                if (!Application.isPlaying)
                    return;

                var label = (FpsLabel)ve;
                (ve as FpsLabel).schedule.Execute(state =>
                {
                    label.m_FpsBuffer[label.m_FpsBufferIndex++] = (int)(1f / Time.unscaledDeltaTime);

                    if (label.m_FpsBufferIndex >= m_FpsRange)
                        label.m_FpsBufferIndex = 0;
                    CalculateFPS(label);
                   
                    label.text = string.Format(display, MathF.Round(label.m_AverageFPS));
                }).Every(100);
            }

            void CalculateFPS(FpsLabel label)
            {
                int sum = 0;
                int highest = 0;
                int lowest = int.MaxValue;
                for (int i = 0; i < m_FpsRange; i++)
                {
                    int fps = label.m_FpsBuffer[i];
                    sum += fps;
                    if (fps > highest)
                        highest = fps;
                    if (fps < lowest)
                        lowest = fps;
                }
                label.m_AverageFPS = (float)sum / m_FpsRange;
            }
        }
    }
}