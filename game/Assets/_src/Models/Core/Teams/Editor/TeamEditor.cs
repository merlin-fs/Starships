using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Ext;
using UnityEngine;
using Common.Core;
using Common.Defs;
using Game.Model;

namespace UnityEditor.Inspector
{

    [CustomPropertyDrawer(typeof(TeamValue), true)]
    public class TeamDefEdit : PropertyDrawer
    {
        //private GlobalTeamsConfig m_GlobalTeamsConfig;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var teams = NeedGlobalsTeams();
            var rect = EditorGUI.PrefixLabel(position, label);
            var names = teams.Teams;
            TeamValue value = (TeamValue)property.boxedValue;

            var idx = teams.GetIndex(value);
            var newIdx = EditorGUI.Popup(rect, idx, teams.Teams);
            if (idx != newIdx)
            {
                value = teams.GetTeam(newIdx);
                property.boxedValue = value;
                EditorUtility.SetDirty(property.serializedObject.targetObject);
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }
        }

        GlobalTeamsConfig NeedGlobalsTeams()
        {
            return GlobalTeamsConfig.Instance;
            /*
            if (!m_GlobalTeamsConfig)
            {
                var asset = AssetDatabase.FindAssets($"t:{nameof(GlobalTeamsConfig)}").First();
                m_GlobalTeamsConfig = AssetDatabase.LoadAssetAtPath<GlobalTeamsConfig>(AssetDatabase.GUIDToAssetPath(asset));
            }
            return m_GlobalTeamsConfig;
            */
        }

    }
}
