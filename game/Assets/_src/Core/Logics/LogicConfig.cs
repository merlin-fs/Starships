using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Common.Defs;

namespace Game.Model.Logics
{
    using Core.Defs;
    using Weapons;
    using static Game.Model.Logics.Logic;

    [CreateAssetMenu(fileName = "Logic", menuName = "Configs/Logic")]
    public class LogicConfig : GameObjectConfig
    {
        public LogicDef Logic = new LogicDef();
        [SerializeReference, ReferenceSelect(typeof(ILogic))]
        private ILogic m_LogicInst;

        [SerializeField, SelectType(typeof(Logic.IPartLogic))]
        List<string> m_Parts = new List<string>();

        protected override void Configurate(Entity prefab, IDefineableContext context)
        {
            base.Configurate(prefab, context);
            Logic.AddComponentData(prefab, context);
        }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            m_Parts.ForEach(part => Logic.AddSupportSystem(Type.GetType(part)));
            Logic.Init();
            m_LogicInst.Init(Logic);
        }
    }
}
