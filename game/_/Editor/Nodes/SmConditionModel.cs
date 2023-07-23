using System;
using UnityEngine;

using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.GraphToolsFoundation.Overdrive;
using System.CodeDom.Compiler;

namespace Common.States.Editor
{

    public struct Unit
    {
        public enum Condition
        {
            Death,
            Damage,
        }

        public enum State
        {
            Init,
            Stop,
        }
    }

    public struct Target
    {
        public enum Condition
        {
            Found,
        }
    }

    public struct Move
    {
        public enum Condition
        {
            Done,
        }
    }

    public struct Weapon
    {
        public enum Condition
        {
            Shot,
        }
    }

    [Serializable]
    [SearcherItem(typeof(SmStencil), SearcherContext.Graph, "Condition")]
    public class SmConditionNodeModel : NodeModel
    {
        [SerializeField]
        Enum m_Condition;

        public override string IconTypeString => "Condition";

        public Enum Condition
        {
            get => m_Condition;
            set => m_Condition = value;
        }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            AddInputPort("in", PortType.Execution, TypeHandle.ExecutionFlow, 
                options: PortModelOptions.Default, 
                orientation: PortOrientation.Vertical);
            AddOutputPort("true", PortType.Execution, TypeHandle.ExecutionFlow, 
                options: PortModelOptions.NoEmbeddedConstant, 
                orientation: PortOrientation.Vertical);
            AddOutputPort("false", PortType.Execution, TypeHandle.ExecutionFlow, 
                options: PortModelOptions.NoEmbeddedConstant, 
                orientation: PortOrientation.Vertical);
        }
    }
}
