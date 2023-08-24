using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Common.Defs;

namespace Game.Model.Logics
{
    using Core.Defs;
    using static Game.Model.Logics.Logic;

    [CreateAssetMenu(fileName = "Logic", menuName = "Configs/Logic")]
    public class LogicConfig : GameObjectConfig
    {
        public LogicDef Logic = new LogicDef();
        protected override void Configurate(Entity prefab, IDefineableContext context)
        {
            base.Configurate(prefab, context);
            Logic.AddComponentData(prefab, context);
        }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            Logic.Init();
        }
    }
}
