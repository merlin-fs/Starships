using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Common.Defs;
using UnityEditor;
using System.Collections.Generic;

namespace Game.Model
{
    using Core.Defs;

    [Serializable]
    public struct TeamValue
    {
        public uint Value;
        private TeamValue(uint value) => Value = value;

        public static implicit operator uint(TeamValue value) => value.Value;
        public static implicit operator TeamValue(uint value) => new TeamValue(value);
    }


    [CreateAssetMenu(fileName = "Teams", menuName = "Configs/Teams")]
    public class GlobalTeamsConfig : GameObjectConfig
    {
        private static GlobalTeamsConfig m_Instance;
        public static GlobalTeamsConfig Instance 
        { 
            get {
                if (m_Instance == null)
                {
#if UNITY_EDITOR
                    var asset = AssetDatabase.FindAssets($"t:{nameof(GlobalTeamsConfig)}").First();
                    m_Instance = AssetDatabase.LoadAssetAtPath<GlobalTeamsConfig>(AssetDatabase.GUIDToAssetPath(asset));
#endif
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

        public string[] GetNames(TeamValue value)
        {
            var list = new List<string>();
            var src = (int)value.Value;
            for (int iter = (int)Mathf.Log(src, 2); iter >= 0; 
                iter = (int)Mathf.Log(src -= (int)Math.Pow(2, iter), 2))
            {
                list.Add(Teams[iter]);
            }
            return list.ToArray();
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