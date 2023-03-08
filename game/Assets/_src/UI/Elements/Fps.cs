using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Elements
{
    public class FpsLabel : Label
    {
        public new class UxmlFactory : UxmlFactory<FpsLabel, UxmlTraits> { }

        private float avgFramerate;
        string display = "{0} FPS";
        int[] m_FpsBuffer = new int[200];
        int m_FpsBufferIndex;
        int m_FpsRange = 200;
        float AverageFPS;

        public new class UxmlTraits : Label.UxmlTraits
        {
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var label = (FpsLabel)ve;
                (ve as FpsLabel).schedule.Execute(state =>
                {
                    label.m_FpsBuffer[label.m_FpsBufferIndex++] = (int)(1f / Time.unscaledDeltaTime);
                    if (label.m_FpsBufferIndex >= label.m_FpsRange)
                        label.m_FpsBufferIndex = 0;
                    CalculateFPS(label);

                    label.text = string.Format(label.display, label.AverageFPS);
                    //float timelapse = Mathf.Round(1f / Time.smoothDeltaTime);
                    //
                }).Every(100);
            }

            void CalculateFPS(FpsLabel label)
            {
                int sum = 0;
                int highest = 0;
                int lowest = int.MaxValue;
                for (int i = 0; i < label.m_FpsRange; i++)
                {
                    int fps = label.m_FpsBuffer[i];
                    sum += fps;
                    if (fps > highest)
                    {
                        highest = fps;
                    }
                    if (fps < lowest)
                    {
                        lowest = fps;
                    }
                }
                label.AverageFPS = (float)sum / label.m_FpsRange;
            }
        }
    }
}