using System;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;

namespace Common.States.Editor
{
    class SmGraphWindow : GraphViewEditorWindow
    {
        [InitializeOnLoadMethod]
        static void RegisterTool()
        {
            ShortcutHelper.RegisterDefaultShortcuts<SmGraphWindow>(SmStencil.toolName);
        }

        [MenuItem("Tools/Game/State machine editor", false)]
        public static void ShowRecipeGraphWindow()
        {
            FindOrCreateGraphWindow<SmGraphWindow>();
        }

        protected override void OnEnable()
        {
            EditorToolName = "State machine";
            base.OnEnable();
        }

        protected override GraphView CreateGraphView()
        {
            return new SmGraphView(this, CommandDispatcher, EditorToolName);
        }

        protected override BlankPage CreateBlankPage()
        {
            var onboardingProviders = new List<OnboardingProvider> { new SmOnboardingProvider() };

            return new BlankPage(CommandDispatcher, onboardingProviders);
        }

        /// <inheritdoc />
        protected override bool CanHandleAssetType(IGraphAssetModel asset)
        {
            return asset is SmGraphAssetModel;
        }

        /// <inheritdoc />
        protected override GraphToolState CreateInitialState()
        {
            var prefs = Preferences.CreatePreferences(EditorToolName);
            return new SmGraphState(GUID, prefs);
        }
    }
}
