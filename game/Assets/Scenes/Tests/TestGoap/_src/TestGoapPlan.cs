using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

using Game.Model.Logics;
using Game.Model;
using Game.Model.Weapons;
using static Game.Model.Logics.Logic;

public class TestGoapPlan : MonoBehaviour
{
    [SerializeField]
    LogicConfig m_Logic;

    [SerializeField]
    Button m_Button;

    // Start is called before the first frame update
    void Start()
    {
        m_Button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        //(var goal, var value) = m_Logic.Logic.GetGoal();
        using var actions = m_Logic.Logic.GetActions(Allocator.Persistent);
        var woldStates = new States(Allocator.Persistent);

        woldStates
            .SetState(Move.Condition.Init, false)
            .SetState(Target.Condition.Dead, false)
            .SetState(Target.Condition.Found, false)
            .SetState(Weapon.Condition.NoAmmo, false);

        var goal = new States(Allocator.Persistent)
            .SetState(Target.Condition.Dead, true)
            .SetState(Move.Condition.Init, true);

        var plan = PlanFinder.Execute(LogicHandle.Null, goal, m_Logic.Logic, actions, woldStates);

        woldStates.SetState(Weapon.Condition.NoAmmo, true);

        plan = PlanFinder.Execute(LogicHandle.Null, goal, m_Logic.Logic, actions, woldStates);
    }
}
