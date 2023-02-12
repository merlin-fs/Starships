using System;
using UnityEngine;

using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.GraphToolsFoundation.Overdrive;
using System.CodeDom.Compiler;

namespace Common.States.Editor
{
    [Serializable]
    [SearcherItem(typeof(SmStencil), SearcherContext.Graph, "Node")]
    class SmNodeModel : NodeModel//ContextNodeModel/** NodeModel*/
    {
        /*
        [SerializeField, HideInInspector]
        int m_InputCount = 1;

        [SerializeField, HideInInspector]
        int m_OutputCount = 1;

        [SerializeField, HideInInspector]
        int m_VerticalInputCount = 1;

        [SerializeField, HideInInspector]
        int m_VerticalOutputCount = 1;

        public int InputCount => m_InputCount;

        public int OutputCount => m_OutputCount;

        public int VerticalInputCount => m_VerticalInputCount;

        public int VerticalOutputCount => m_VerticalOutputCount;

        */

        
        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            AddInputPort("in", PortType.Execution, TypeHandle.ExecutionFlow,
                options: PortModelOptions.Default,
                orientation: PortOrientation.Vertical);
            AddOutputPort("out", PortType.Execution, TypeHandle.ExecutionFlow,
                options: PortModelOptions.NoEmbeddedConstant,
                orientation: PortOrientation.Vertical);

            //this.AddExecutionInputPort("In", orientation: PortOrientation.Vertical);

            //this.AddDataOutputPort("Out 1", TypeHandle.Vector2, options: PortModelOptions.NoEmbeddedConstant);
            //this.AddDataOutputPort("Out 2", TypeHandle.Vector2, options: PortModelOptions.NoEmbeddedConstant);
            //this.AddPlaceHolderPort(PortDirection.None, "Out 1", orientation: PortOrientation.Vertical);
            //this.AddPlaceHolderPort(PortDirection.None, "Out 2", orientation: PortOrientation.Vertical);
            //this.AddExecutionOutputPort("Out 1", orientation: PortOrientation.Vertical);
            //this.AddExecutionOutputPort("Out 2", orientation: PortOrientation.Vertical);
        }
        

        /*
        public void AddPort(PortOrientation orientation, PortDirection direction)
        {
            if (orientation == PortOrientation.Horizontal)
            {
                if (direction == PortDirection.Input)
                    m_InputCount++;
                else
                    m_OutputCount++;
            }
            else
            {
                if (direction == PortDirection.Input)
                    m_VerticalInputCount++;
                else
                    m_VerticalOutputCount++;
            }

            DefineNode();
        }
        */

        /*
        public void RemovePort(PortOrientation orientation, PortDirection direction)
        {
            if (orientation == PortOrientation.Horizontal)
            {
                if (direction == PortDirection.Input)
                    m_InputCount--;
                else
                    m_OutputCount--;
            }
            else
            {
                if (direction == PortDirection.Input)
                    m_VerticalInputCount--;
                else
                    m_VerticalOutputCount--;
            }

            DefineNode();
        }
        */
    }
}
