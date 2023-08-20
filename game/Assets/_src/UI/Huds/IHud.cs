using System;
using Unity.Mathematics;
using UnityEngine.UIElements;

namespace Game.UI.Huds
{
    public interface IHud
    {
        void SetPosition(float3 position);
        void Initialize(HudManager hudManager, VisualElement element);
    }
}
