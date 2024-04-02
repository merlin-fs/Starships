using System;
using UnityEngine.UIElements;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEditor;

namespace Common.States.Editor
{
    class SmConditionNode : Node //Node //CollapsibleInOutNode
    {
        public static readonly string paramContainerPartName = "ge-icon-title-progress";
        public static readonly string topPortContainerPartName = "top-vertical-port-container";
        public static readonly string bottomPortContainerPartName = "bottom-vertical-port-container";


        protected override void BuildPartList()
        {
            PartList.AppendPart(VerticalPortContainerPart.Create(topPortContainerPartName, PortDirection.Input, Model, this, ussClassName));
            PartList.AppendPart(SmConditionPart.Create(titleContainerPartName, Model, this, ussClassName));
            PartList.AppendPart(VerticalPortContainerPart.Create(bottomPortContainerPartName, PortDirection.Output, Model, this, ussClassName));
        }

        protected override void PostBuildUI()
        {
            base.PostBuildUI();
        }

        protected override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            if (Model is SmConditionNodeModel mixNodeModel)
            {

                if (evt.menu.MenuItems().Count > 0)
                    evt.menu.AppendSeparator();

                /*
                evt.menu.AppendAction($"Add Ingredient", action: action =>
                {
                    CommandDispatcher.Dispatch(new AddPortCommand(new[] { mixNodeModel }));
                });

                evt.menu.AppendAction($"Remove Ingredient", action: action =>
                {
                    CommandDispatcher.Dispatch(new RemovePortCommand(new[] { mixNodeModel }));
                });
                */
            }
        }
    }
}
