using UnityEngine;
using Unity.Entities;
using Common.Defs;
using Game.Core.Defs;
using static Game.Model.Logics.Logic;

namespace Game.Model.Logics
{
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
