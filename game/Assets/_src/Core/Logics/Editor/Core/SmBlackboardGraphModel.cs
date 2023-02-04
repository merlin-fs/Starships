using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine.GraphToolsFoundation.Overdrive;

using UnityEngine;

namespace Common.States.Editor
{
    class SmBlackboardGraphModel : BlackboardGraphModel
    {
        internal static readonly string k_Section = "Condition";

        /// <inheritdoc />
        public SmBlackboardGraphModel(IGraphAssetModel graphAssetModel) : base(graphAssetModel) { }

        /// <inheritdoc />
        public override string GetBlackboardTitle()
        {
            return "State machine";
        }

        /// <inheritdoc />
        public override IEnumerable<string> SectionNames =>
            GraphModel == null ? Enumerable.Empty<string>() : new List<string>() { k_Section };

        public override IEnumerable<IVariableDeclarationModel> GetSectionRows(string sectionName)
        {
            if (sectionName == k_Section)
            {
                return GraphModel?.VariableDeclarations?.Where(v => v.DataType == SmStencil.Condition) ??
                    Enumerable.Empty<IVariableDeclarationModel>();
            }
            
            return Enumerable.Empty<IVariableDeclarationModel>();
        }
    }
}
