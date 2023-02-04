using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine.UIElements;

namespace Common.States.Editor
{
    class SmOnboardingProvider : OnboardingProvider
    {
        public override VisualElement CreateOnboardingElements(CommandDispatcher store)
        {
            var template = new GraphTemplate<SmStencil>(SmStencil.k_GraphName);
            return AddNewGraphButton<SmGraphAssetModel>(template);
        }
    }
}
