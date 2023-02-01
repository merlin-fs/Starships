using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

using Game.Model.Logics;
using Game.Model;
using Game.Model.Weapons;
using static Game.Model.Logics.Logic;
using System.Linq;

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
        PlanFinder.Init();
    }

    void OnClick()
    {
        var woldStates = new States(Allocator.Persistent);
        woldStates
            .SetState(Move.Condition.Init, true)
            .SetState(Target.Condition.Dead, false)
            .SetState(Target.Condition.Found, false)
            .SetState(Weapon.Condition.NoAmmo, false)
            .SetState(Weapon.Condition.HasAmo, true);


        var goal = new States(Allocator.Persistent)
            .SetState(Target.Condition.Dead, true)
            .SetState(Move.Condition.Init, true);

        var sw = new System.Diagnostics.Stopwatch();
        NativeArray<LogicHandle> plan = default;

        sw.Start();
        plan = PlanFinder.Execute(0, m_Logic.Logic, goal, woldStates, Allocator.Persistent);
        Debug.Log($"{string.Join(", ", plan.ToArray().Select(i => $"{i}"))}, {sw.Elapsed}");
        plan.Dispose();

        woldStates.SetState(Weapon.Condition.NoAmmo, true);

        sw.Restart();
        plan = PlanFinder.Execute(0, m_Logic.Logic, goal, woldStates, Allocator.Persistent);
        Debug.Log($"{string.Join(", ", plan.ToArray().Select(i => $"{i}"))}, {sw.Elapsed}");
        plan.Dispose();

        woldStates
            .SetState(Weapon.Condition.NoAmmo, true)
            .SetState(Weapon.Condition.HasAmo, false);

        sw.Restart();
        plan = PlanFinder.Execute(0, m_Logic.Logic, goal, woldStates, Allocator.Persistent);
        Debug.Log($"{string.Join(", ", plan.ToArray().Select(i => $"{i}"))}, {sw.Elapsed}");
        plan.Dispose();
    }
}
