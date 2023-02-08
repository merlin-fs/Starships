using System;
using UnityEngine;


namespace Game.Model.Logics
{
    using Weapons;
    using Core.Defs;
    using Common.Defs;
    using Unity.Entities;

    [CreateAssetMenu(fileName = "Logic", menuName = "Configs/Logic")]
    public class LogicConfig : GameObjectConfig
    {
        public Logic.LogicDef Logic = new Logic.LogicDef();

        protected override void Configurate(Entity prefab, IDefineableContext context)
        {
            base.Configurate(prefab, context);
            Logic.AddComponentData(prefab, context);
        }

        public override void OnAfterDeserialize()
        {
            Logic.AddAction(Move.Action.Init)
                .AddPreconditions(Move.State.Init, false)
                .AddEffect(Move.State.Init, true)
                .Cost(1);

            Logic.AddAction(Target.Action.Find)
                .AddPreconditions(Target.State.Found, false)
                .AddPreconditions(Weapon.State.NoAmmo, false)
                .AddEffect(Target.State.Found, true)
                .Cost(1);

            Logic.AddAction(Weapon.Action.Shoot)
                .AddPreconditions(Target.State.Found, true)
                .AddPreconditions(Weapon.State.NoAmmo, false)
                .AddEffect(Target.State.Dead, true)
                .Cost(1);

            Logic.AddAction(Weapon.Action.bomb)
                .AddEffect(Target.State.Dead, true)
                .Cost(8);

            Logic.AddAction(Weapon.Action.Reload)
                .AddPreconditions(Weapon.State.NoAmmo, true)
                .AddPreconditions(Weapon.State.HasAmo, true)
                .AddEffect(Weapon.State.NoAmmo, false)
                .Cost(2);

            Logic.AddGoal(Target.State.Dead, true);
        }
    }
}
