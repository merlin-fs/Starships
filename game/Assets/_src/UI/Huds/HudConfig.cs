using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Huds
{
    public class HudConfig : ScriptableObject
    {
        [SelectType(typeof(Hud))]
        [SerializeField] private string hud;
        [SerializeField] private VisualTreeAsset template;
        [SerializeField] private StyleSheet[] styles;

        public Type Hud => Type.GetType(hud);
        public VisualTreeAsset Template => template;
        public StyleSheet[] Styles => styles;
    }
}