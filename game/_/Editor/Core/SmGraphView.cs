
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Common.States.Editor
{
    class SmGraphView : GraphView
    {
        public SmGraphView(GraphViewEditorWindow window, CommandDispatcher commandDispatcher, string graphViewName)
            : base(window, commandDispatcher, graphViewName)
        {
            SetupZoom(0.05f, 5.0f, 5.0f);
        }
    }
}
