using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Common.Defs;
using Common.Core;
using UnityEditor;

namespace Game.Model
{
    [Serializable]
    public struct TeamValue
    {
        public uint Value;
        private TeamValue(uint value) => Value = value;

        public static implicit operator uint(TeamValue value) => value.Value;
        public static implicit operator TeamValue(uint value) => new TeamValue(value);
    }


    [CreateAssetMenu(fileName = "Teams", menuName = "Configs/Teams")]
    public class GlobalTeamsConfig : ScriptableConfig
    {
        private static GlobalTeamsConfig m_Instance;
        public static GlobalTeamsConfig Instance 
        { 
            get {
                if (m_Instance == null)
                {
                    var asset = AssetDatabase.FindAssets($"t:{nameof(GlobalTeamsConfig)}").First();
                    m_Instance = AssetDatabase.LoadAssetAtPath<GlobalTeamsConfig>(AssetDatabase.GUIDToAssetPath(asset));
                }
                return m_Instance;
            }
            private set => m_Instance = value;
        }

        [SerializeField]
        private string[] m_Teams;

        public string[] Teams => m_Teams;

        private void OnEnable()
        {
            Instance = this;
        }

        public string GetName(TeamValue value)
        {
            var idx = GetIndex(value);
            return idx < -1 
                ? "null" 
                : Teams[idx];
        }

        public int GetIndex(TeamValue value)
        {
            return (int)Mathf.Log(value.Value, 2);
        }

        public TeamValue GetTeam(string value)
        {
            var idx = Array.IndexOf(m_Teams, value);
            return idx == -1 
                ? 0 
                : GetTeam(idx);
        }

        public TeamValue GetTeam(int value)
        {
            return (uint)Math.Pow(2, value);
        }

        protected override void Configurate(Entity prefab, IDefineableContext context)
        {
        }
    }
}