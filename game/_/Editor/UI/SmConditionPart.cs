using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;

using UnityEngine.UIElements;

namespace Common.States.Editor
{
    public class SmConditionPart : BaseModelUIPart
    {
        public static readonly string ussClassName = "title-condition";
        public static readonly string labelName = "type";

        public static SmConditionPart Create(string name, IGraphElementModel model, IModelUI modelUI, string parentClassName)
        {
            if (model is INodeModel)
            {
                return new SmConditionPart(name, model, modelUI, parentClassName);
            }

            return null;
        }

        VisualElement ConditionContainer { get; set; }
        PopupField<EnumInfo> ConditionLabel { get; set; }

        public override VisualElement Root => ConditionContainer;

        SmConditionPart(string name, IGraphElementModel model, IModelUI ownerElement, string parentClassName)
            : base(name, model, ownerElement, parentClassName)
        {
        }

        struct EnumInfo
        {
            public Type EnumType;
            public object Item;
            public string Caption;
        }

        protected override void BuildPartUI(VisualElement container)
        {
            if (m_Model is SmConditionNodeModel model)
            {
                ConditionContainer = new VisualElement { name = PartName };
                ConditionContainer.AddToClassList(ussClassName);
                ConditionContainer.AddToClassList(m_ParentClassName.WithUssElement(PartName));
                container.Add(ConditionContainer);

                var icon = new VisualElement();
                icon.AddToClassList(ussClassName.WithUssElement("icon"));
                icon.AddToClassList(m_ParentClassName.WithUssElement("icon"));
                if (!string.IsNullOrEmpty(model.IconTypeString))
                {
                    icon.AddToClassList(ussClassName.WithUssElement("icon").WithUssModifier(model.IconTypeString));
                    icon.AddToClassList(m_ParentClassName.WithUssElement("icon").WithUssModifier(model.IconTypeString));
                }
                ConditionContainer.Add(icon);

                var list = Populate(typeof(Unit))
                    .Union(Populate(typeof(Weapon)))
                    .Union(Populate(typeof(Target)))
                    .Union(Populate(typeof(Move)));

                IEnumerable<Type> Populate(Type type)
                {
                    return type.GetNestedTypes()
                        .Where(t => t.IsEnum && t.Name == "Condition");
                }

                var names = list.SelectMany(t => 
                    Enum.GetNames(t).
                        Select(s => new EnumInfo
                        {
                            EnumType = t,
                            Item = Enum.Parse(t, s),
                            Caption = $"{s} ({t.DeclaringType.Name})",
                        }))
                    .ToList();

                ConditionLabel = new PopupField<EnumInfo>(names, 0, 
                    i => i.Item.ToString(), 
                    i => i.Caption
                    );

                ConditionLabel.RegisterCallback<ChangeEvent<string>>(OnChangeCondition);
                ConditionLabel.AddToClassList(ussClassName.WithUssElement("temperature"));
                ConditionLabel.AddToClassList(m_ParentClassName.WithUssElement("temperature"));
                ConditionContainer.Add(ConditionLabel);
            }
        }

        void OnChangeCondition(ChangeEvent<string> evt)
        {
            if (m_Model is SmConditionNodeModel model)
            {
                //m_OwnerElement.CommandDispatcher.Dispatch(new SetTemperatureCommand(v, model));
            }
        }

        protected override void PostBuildPartUI()
        {
            base.PostBuildPartUI();

            var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.unity.graphtools.foundation/Samples/Recipes/Editor/UI/Stylesheets/BakeNodePart.uss");
            if (stylesheet != null)
            {
                ConditionContainer.styleSheets.Add(stylesheet);
            }

            stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/_src/StateMachine/Editor/UI/Condition.uss");
            if (stylesheet != null)
            {
                ConditionContainer.styleSheets.Add(stylesheet);
            }

        }

        protected override void UpdatePartFromModel()
        {
            if (m_Model is SmConditionNodeModel model)
            {
                //!!!ConditionLabel.SetValueWithoutNotify(model.Condition?.ToString());
            }
        }
    }
}
