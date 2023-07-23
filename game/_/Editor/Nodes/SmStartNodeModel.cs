using System;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Common.States.Editor
{
    [Serializable]
    [SearcherItem(typeof(SmStencil), SearcherContext.Graph, "Start")]
    class SmStartNodeModel : NodeModel
    {
        public SmStartNodeModel()
        {
            this.SetCapability(UnityEditor.GraphToolsFoundation.Overdrive.Capabilities.Collapsible, false);
        }
       
        protected override void OnDefineNode()
        {
            base.OnDefineNode();
            this.AddExecutionOutputPort("Always", "out", PortOrientation.Horizontal);
            //AddOutputPort("Always", PortType.Data, TypeHandle.ExecutionFlow/* SmStencil.Condition*/, options: PortModelOptions.NoEmbeddedConstant);
        }
    }
}
