using System.Linq;

using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;

using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Common.States.Editor
{
    class SmStencil : Stencil
    {
        [SerializeField]
        string test;

        public class State
        {
            State() { }
        }

        //TypeHandleHelpers.GenerateCustomTypeHandle("Condition");
        public static TypeHandle Condition { get; } = TypeHandleHelpers.GenerateCustomTypeHandle(typeof(State), "__EXECUTIONFLOW");

        public static string toolName = "State machine Editor";

        public override string ToolName => toolName;

        /// <inheritdoc />
        public override IBlackboardGraphModel CreateBlackboardGraphModel(IGraphAssetModel graphAssetModel)
        {
            return new SmBlackboardGraphModel(graphAssetModel);
        }

        public static readonly string k_GraphName = "StateMachine";

        public override void PopulateBlackboardCreateMenu(string sectionName, GenericMenu menu, CommandDispatcher commandDispatcher)
        {
            menu.AddItem(new GUIContent("Condition"), false, () =>
            {
                CreateVariableDeclaration(Condition.Identification, Condition);
            });

            void CreateVariableDeclaration(string name, TypeHandle type)
            {
                var finalName = name;
                var i = 0;

                // ReSharper disable once AccessToModifiedClosure
                while (commandDispatcher.State.WindowState.GraphModel.VariableDeclarations.Any(v => v.Title == finalName))
                    finalName = name + i++;

                commandDispatcher.Dispatch(new CreateGraphVariableDeclarationCommand(finalName, true, type));
            }
        }

    }
}
