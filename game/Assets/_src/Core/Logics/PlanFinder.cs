using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public partial struct PlanFinder
        {
            public static NativeArray<LogicHandle> Execute(int threadIdx, LogicAspect logic, Goal goal, 
                AllocatorManager.AllocatorHandle allocator)
            {
                var path = Search(threadIdx, logic, goal, allocator);
                return path;
            }

            private unsafe static NativeArray<LogicHandle> Search(int threadIdx, LogicAspect logic,
                Goal goal, AllocatorManager.AllocatorHandle allocator)
            {
                InitFinder(threadIdx);
                try
                {
                    var root = new Node(LogicHandle.Null, 0)
                    {
                        HeuristicCost = 0.0f
                    };

                    if (!logic.HasWorldState(goal.State, goal.Value))
                        root.Goals.Add(goal.State, goal.Value);

                    var costs = GetCosts(threadIdx);
                    costs.Add(LogicHandle.Null, root);
                    var queue = GetQueue(threadIdx);

                    queue.Push(root);
                    while (queue.Pop(out Node node))
                    {
                        if (node.Goals.Count == 0)
                            return ShortestPath(threadIdx, node.Handle, allocator);
                        IdentifySuccessors(threadIdx, node, logic);
                    }

                    return new NativeList<LogicHandle>(1, allocator).AsArray();
                }
                finally
                {
                    FinishFinder(threadIdx);
                }
            }

            private static void IdentifySuccessors(int threadIdx, Node node, LogicAspect logic)
            {
                var costs = GetCosts(threadIdx);
                var queue = GetQueue(threadIdx);
                var hierarchy = GetHierarchy(threadIdx);

                foreach (var iter in node.Goals)
                {
                    foreach (var pt in logic.Def.GetActionsFromGoal(GoalHandle.FromHandle(iter.Key, iter.Value)))
                    {
                        logic.Def.TryGetAction(pt, out GoapAction action);
                        if (!costs.TryGetValue(action.Handle, out Node next))
                        {
                            next = new Node(action.Handle, action.Cost);
                            costs.Add(action.Handle, next);
                        }
                        next.Goals.Clear();

                        foreach (var nextGoal in action.GetPreconditions().GetReadOnly())
                            if (!logic.HasWorldState(nextGoal.Key, nextGoal.Value))
                                next.Goals.Add(nextGoal.Key, nextGoal.Value);

                        foreach (var nextGoal in node.Goals)
                        {
                            if (iter.Key == nextGoal.Key && iter.Value == nextGoal.Value)
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

            private static NativeArray<LogicHandle> ShortestPath(int threadIdx, LogicHandle v, AllocatorManager.AllocatorHandle allocator)
            {
                var hierarchy = GetHierarchy(threadIdx);
                var path = new NativeList<LogicHandle>(hierarchy.Count, allocator);
                while (!v.Equals(LogicHandle.Null))
                {
                    if (!hierarchy.TryGetValue(v, out LogicHandle test))
                    {
                        path.Dispose();
                        return new NativeList<LogicHandle>(1, allocator).AsArray();
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
                public LogicHandle Handle { get; }

                public NativeHashMap<LogicHandle, bool> Goals { get; }

                public Node(LogicHandle source, float cost)
                {
                    Goals = new NativeHashMap<LogicHandle, bool>(5, Allocator.TempJob);
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