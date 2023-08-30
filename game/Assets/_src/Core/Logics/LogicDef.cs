using System;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Game.Core;

using UnityEngine;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public partial class LogicDef
        {
            //TODO: временное поле, будет заменено на конфиг
            [SerializeReference, ReferenceSelect(typeof(ILogic))]
            private ILogic logicInst;
            
            public bool IsValid => m_Actions.Count > 0;
            public  EnumHandle InitializeAction { get; private set; }

            public void Init()
            {
                m_StateMapping.Clear();
                m_Actions.Clear();
                m_Goal.Clear();
                m_Effects.Dispose();
                m_Effects = new Map<GoalHandle, EnumHandle>(10, Allocator.Persistent, true);
                
                var types = typeof(IStateData).GetDerivedTypes(true)
                    .SelectMany(t => t.GetNestedTypes())
                    .Where(t => t.IsEnum && t.Name == "State");

                foreach (var iter in types)
                {
                    foreach (var e in iter.GetEnumValues())
                    {
                        var value = EnumHandle.Manager.FromEnum(e);
                        m_StateMapping.Add(value, new WorldActionData { Index = m_StateMapping.Count });
                    }
                }
                InitInst();
            }

            public void SetInitializeAction<T>(T value)
                where T : struct, IConvertible
            {
                InitializeAction = EnumHandle.FromEnum(value);
            }

            private void InitInst()
            {
                logicInst?.Init(this);
            }
        }
    }
}