using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

using Game.Model.Logics;
using static Game.Model.Logics.Logic;

public class TestGoapPlan : MonoBehaviour
{
    [SerializeField]
    LogicConfig m_Logic;

    [SerializeField]
    Button m_Button;

    private Map<int, int> m_Map = new Map<int, int>(10, Allocator.Persistent, false);
    private Dictionary<int, int> m_Dic = new Dictionary<int, int>(10);

    private int size = 10000000;

    // Start is called before the first frame update
    void Start()
    {
        m_Button.onClick.AddListener(OnClick);
        PlanFinder.Init();
        FillDic();
        FillMap();
    }

    void FillMap()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        for (int i = 0; i < size; i++)
        {
            m_Map.Add(i, i);
        }
        Debug.Log($"fill map: {sw.Elapsed}");
    }

    void FillDic()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        for (int i = 0; i < size; i++)
        {
            m_Dic.Add(i, i);
        }
        Debug.Log($"fill dic: {sw.Elapsed}");
    }

    void TestRBTree()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        for (int i = 0; i < 10; i++)
        {
            var idx = UnityEngine.Random.Range(0, size);
            sw.Restart();
            bool found = m_Map.TryGetValue(idx, out int value);
            Debug.Log($"map {found}: {sw.Elapsed}");
        }
    }

    void TestDic()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        for (int i = 0; i < 10; i++)
        {
            var idx = UnityEngine.Random.Range(0, size);
            sw.Restart();
            bool found = m_Dic.TryGetValue(idx, out int value);
            Debug.Log($"dic {found}: {sw.Elapsed}");
        }
    }

    void OnClick()
    {
        TestDic();
        TestRBTree();
        /*
        var woldStates = new States(Allocator.Persistent);
        woldStates
            .SetState(Move.State.Init, true)
            .SetState(Target.State.Dead, false)
            .SetState(Target.State.Found, false)
            .SetState(Weapon.State.NoAmmo, false)
            .SetState(Weapon.State.HasAmo, true);


        var goal = new States(Allocator.Persistent)
            .SetState(Target.State.Dead, true)
            .SetState(Move.State.Init, true);

        var sw = new System.Diagnostics.Stopwatch();
        NativeArray<LogicHandle> plan = default;

        sw.Start();
        plan = PlanFinder.Execute(0, m_Logic.Logic, goal, woldStates, Allocator.Persistent);
        Debug.Log($"{string.Join(", ", plan.ToArray().Select(i => $"{i}"))}, {sw.Elapsed}");
        plan.Dispose();

        woldStates.SetState(Weapon.State.NoAmmo, true);

        sw.Restart();
        plan = PlanFinder.Execute(0, m_Logic.Logic, goal, woldStates, Allocator.Persistent);
        Debug.Log($"{string.Join(", ", plan.ToArray().Select(i => $"{i}"))}, {sw.Elapsed}");
        plan.Dispose();

        woldStates
            .SetState(Weapon.State.NoAmmo, true)
            .SetState(Weapon.State.HasAmo, false);

        sw.Restart();
        plan = PlanFinder.Execute(0, m_Logic.Logic, goal, woldStates, Allocator.Persistent);
        Debug.Log($"{string.Join(", ", plan.ToArray().Select(i => $"{i}"))}, {sw.Elapsed}");
        plan.Dispose();
        */
    }
}
