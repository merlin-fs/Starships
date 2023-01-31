using System;
using UnityEngine;


namespace Game.Model.Logics
{
    using Weapons;
    using Core.Defs;

    [CreateAssetMenu(fileName = "Logic", menuName = "Configs/Logic")]
    public class LogicConfig : GameObjectConfig
    {
        public Logic.LogicDef Logic = new Logic.LogicDef();

        public override void OnAfterDeserialize()
        {
            Logic.AddAction(Move.State.Init)
                .AddPreconditions(Move.Condition.Init, false)
                .AddEffect(Move.Condition.Init, true);

            Logic.AddAction(Target.State.Find)
                .AddPreconditions(Target.Condition.Found, false)
                .AddEffect(Target.Condition.Found, true);

            Logic.AddAction(Weapon.State.Shoot)
                .AddPreconditions(Target.Condition.Found, true)
                .AddPreconditions(Weapon.Condition.NoAmmo, false)
                .AddEffect(Target.Condition.Dead, true);

            Logic.AddAction(Weapon.State.bomb)
                .AddEffect(Target.Condition.Dead, true);

            Logic.AddAction(Weapon.State.Reload)
                .AddPreconditions(Weapon.Condition.NoAmmo, true)
                .AddEffect(Weapon.Condition.NoAmmo, false);

            Logic.AddGoal(Target.Condition.Dead, true);
        }
    }
}
