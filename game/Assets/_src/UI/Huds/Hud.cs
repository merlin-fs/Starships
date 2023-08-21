using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Huds
{
    public abstract partial class Hud
    {
        protected Manager HudManager { get; private set; }
        protected VisualElement Element { get; private set; }
        
        private void Initialize(Manager hudManager) => HudManager = hudManager;
        protected virtual void Configure(VisualElement element) => Element = element;
        protected virtual void SetPosition(float3 position, VisualElement element)
        {
            float scale = 1 + HudManager.WorldCamera.transform.localPosition.z / 200;
            position.y += 1.1f; 
            Vector2 newPosition = RuntimePanelUtils.CameraTransformWorldToPanel(element.panel, position, HudManager.WorldCamera);
            newPosition.x = (newPosition.x - element.layout.width / 2);
            element.transform.scale = new Vector3(scale, scale, scale);
            element.transform.position = newPosition;
        }
    }
}