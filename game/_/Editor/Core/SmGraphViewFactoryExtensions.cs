using System;
using UnityEngine;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Common.States.Editor
{
    [GraphElementsExtensionMethodsCache(typeof(SmGraphView))]
    public static class SmGraphViewFactoryExtensions
    {
        public static IModelUI CreateNode(this ElementBuilder elementBuilder, CommandDispatcher dispatcher, SmConditionNodeModel model)
        {
            IModelUI ui = new SmConditionNode();
            ui.SetupBuildAndUpdate(model, dispatcher, elementBuilder.View, elementBuilder.Context);
            return ui;
        }
    }
}
