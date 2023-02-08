using System;
using UnityEngine;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Common.States.Editor
{
    [Serializable]
    [SearcherItem(typeof(SmStencil), SearcherContext.Graph, "Action")]
    public class SmActionModel : NodeModel
    {
        public override string IconTypeString => "Condition";

        protected override void OnDefineNode()
        {
            base.OnDefineNode();
        }
    }
}
