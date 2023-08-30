using System;
using System.Collections.Generic;

using Game.Core;

using Unity.Collections;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public partial struct PlanFinder
        {
            public static NativeArray<Plan> Execute(int threadIdx, Logic.Aspect logic, Goal goal, 
                AllocatorManager.AllocatorHandle allocator)
            {
                var path = Search(threadIdx, logic, goal, allocator);
                return path;
            }

            private unsafe static NativeArray<Plan> Search(int threadIdx, Logic.Aspect logic,
                Goal goal, AllocatorManager.AllocatorHandle allocator)
            {
                InitFinder(threadIdx);
                try
                {
                    var root = new Node(EnumHandle.Null, 0)
                    {
                        HeuristicCost = 0.0f
                    };

                    if (!logic.HasWorldState(goal.State, goal.Value))
                        root.Goals.Add(goal.State, goal.Value);

                    var costs = GetCosts(threadIdx);
                    costs.Add(EnumHandle.Null, root);
                    var queue = GetQueue(threadIdx);

                    queue.Push(root);
                    var store = root.Handle; 
                    while (queue.Pop(out Node node))
                    {
                        store = node.Handle;
                        if (node.Goals.Count == 0)
                            return ShortestPath(threadIdx, node.Handle, allocator);
                        IdentifySuccessors(threadIdx, node, logic);
                    }
                    return ShortestPath(threadIdx, store, allocator);
                    //return new NativeList<Plan>(1, Allocator.Persistent).AsArray();
                }
                finally
                {
                    FinishFinder(threadIdx);
                }
            }

            private static void IdentifySuccessors(int threadIdx, Node node, Logic.Aspect logic)
            {
                var costs = GetCosts(threadIdx);
                var queue = GetQueue(threadIdx);
                var hierarchy = GetHierarchy(threadIdx);

                using var goals = node.Goals.GetKeyValueArrays(Allocator.Temp);
                for (int i = 0; i < goals.Length; i++)
                //foreach (var iter in goals)
                {
                    foreach (var pt in logic.Def.GetActionsFromGoal(GoalHandle.FromHandle(goals.Keys[i], goals.Values[i])))
                    {
                        logic.Def.TryGetAction(pt, out GoapAction action);
                        if (!costs.TryGetValue(action.Handle, out Node next))
                        {
                            next = new Node(action.Handle, action.Cost);
                            costs.Add(action.Handle, next);
                        }
                        else 
                            continue;
                        next.Goals.Clear();

                        foreach (var nextGoal in action.GetPreconditions().GetReadOnly())
                            if (!logic.HasWorldState(nextGoal.Key, nextGoal.Value))
                                next.Goals.Add(nextGoal.Key, nextGoal.Value);

                        foreach (var nextGoal in node.Goals)
                        {
                            if (goals.Keys[i] == nextGoal.Key && goals.Values[i] == nextGoal.Value)
                                continue;
                            if (!logic.HasWorldState(nextGoal.Key, nextGoal.Value))
                                if (!next.Goals.ContainsKey(nextGoal.Key))
                                    next.Goals.Add(nextGoal.Key, nextGoal.Value);
                        }

                        float heuristicCost = node.HeuristicCost.Value + next.Cost;
                        next.HeuristicCost = heuristicCost;
                        costs[action.Handle] = next;
                        queue.Push(next);
                        hierarchy[action.Handle] = node.Handle;
                    }
                }
            }

            private static NativeArray<Plan> ShortestPath(int threadIdx, EnumHandle v, AllocatorManager.AllocatorHandle allocator)
            {
                var hierarchy = GetHierarchy(threadIdx);
                var path = new NativeList<Plan>(hierarchy.Count, Allocator.Persistent);
                while (!v.Equals(EnumHandle.Null))
                {
                    if (!hierarchy.TryGetValue(v, out EnumHandle test))
                    {
                        path.Dispose();
                        return new NativeList<Plan>(1, Allocator.Persistent).AsArray();
                    }
                    else
                    {
                        path.Add(v);
                        v = test;
                    }

                    if (path.Length > hierarchy.Count)
                    {
                        break;
                    }

                };
                path.Reverse();
                return path.AsArray();
            }

            public struct Node : IEquatable<Node>
            {
                public float? HeuristicCost { get; set; }
                public float Cost { get; }
                public EnumHandle Handle { get; }

                public NativeHashMap<EnumHandle, bool> Goals { get; }

                public Node(EnumHandle source, float cost)
                {
                    Goals = new NativeHashMap<EnumHandle, bool>(5, Allocator.TempJob);
                    Handle = source;
                    HeuristicCost = null;
                    Cost = cost;
                }

                public void Dispose()
                {
                    Goals.Dispose();
                }

                public override int GetHashCode()
                {
                    return Handle.GetHashCode();
                }
                public bool Equals(Node other)
                {
                    return Handle == other.Handle;
                }
            }
        }
    }
}