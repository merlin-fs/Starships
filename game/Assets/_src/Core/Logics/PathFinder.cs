using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public partial struct PathFinder
        {
            private LogicHandle m_EndNode, m_StartNode;
            private NativeParallelHashMap<LogicHandle, Node> m_Costs;
            private SortedNativeHash<Node> m_Queue;
            private NativeParallelHashMap<LogicHandle, LogicHandle> m_Hierarchy;

            public static NativeArray<LogicHandle> Execute(LogicHandle source, LogicHandle target,
                NativeArray<GoapAction> availableActions,
                States worldStates)
            {
                var finder = new PathFinder()
                {
                    m_EndNode = target,
                    m_StartNode = source,
                };
                var path = finder.Search(source, target, availableActions, worldStates);
                return path;
            }

            private NativeArray<LogicHandle> Search(LogicHandle source, LogicHandle target, 
                NativeArray<GoapAction> availableActions, States worldStates)
            {
                var states = new States(Allocator.Persistent);
                //foreach (var state in worldStates) states.Add(state.Key, state.Value);
                
                m_Hierarchy = new NativeParallelHashMap<LogicHandle, LogicHandle>(100, Allocator.TempJob);
                m_Costs = new NativeParallelHashMap<LogicHandle, Node>(100, Allocator.TempJob)
                {
                    {
                        source,
                        new Node(source, 0)
                        {
                            HeuristicCost = 0.0f
                        }
                    }
                };
                m_Queue = new SortedNativeHash<Node>(100, Allocator.TempJob,
                    (Node i1, Node i2) =>
                    {
                        float result = i1.HeuristicCost.Value - i2.HeuristicCost.Value;
                        return Math.Sign(result);
                    }
                );


                m_Queue.Push(m_Costs[source]);
                try
                {
                    while (m_Queue.Pop(out Node node))
                    {
                        //if (states.TryGetValue(target, out bool value) && value) 
                        //if (node.States.TryGetValue(target, out bool value) && value) 
                        //if (node.Equals(target))
                        {
                            return ShortestPath(node.Handle); // rebuilding path
                        }
                        IdentifySuccessors(node, availableActions, states);
                    }
                    return new NativeArray<LogicHandle>(1, Allocator.TempJob);
                }
                finally
                {
                }
            }

            private void IdentifySuccessors(Node node, NativeArray<GoapAction> actions, States states)
            {
                for (int i = 0; i < actions.Length; i++)
                {
                    var current = actions[i];
                    if (node.Handle == current.Handle)
                        continue;

                    if (!current.CanTransition(states))
                        continue; 

                    if (!m_Costs.TryGetValue(current.Handle, out Node next))
                    {
                        current.ApplyEffect(states);
                        next = new Node(current.Handle, current.Cost);
                        m_Costs.Add(current.Handle, next);
                    }

                    float heuristicCost = node.HeuristicCost.Value + next.Cost;
                    if (!next.HeuristicCost.HasValue || (heuristicCost < next.Cost))
                    {
                        next.HeuristicCost = heuristicCost;
                        if (!m_Queue.Contains(next))
                        {
                            m_Queue.Push(next);
                            m_Hierarchy[current.Handle] = node.Handle;
                            m_Costs[current.Handle] = next;
                        }
                    }
                }
            }

            private NativeArray<LogicHandle> ShortestPath(LogicHandle v)
            {
                var path = new NativeList<LogicHandle>(m_Hierarchy.Count(), Allocator.TempJob);
                try
                {
                    while (!v.Equals(m_StartNode))
                    {
                        if (!m_Hierarchy.TryGetValue(v, out LogicHandle test))
                        {
                            return new NativeArray<LogicHandle>(1, Allocator.TempJob);
                        }
                        else
                        {
                            path.Add(v);
                            v = test;
                        }

                        if (path.Length > m_Hierarchy.Count())
                        {
                            break;
                        }

                    };
                    path.Add(m_StartNode);
                    path.Reverse();
                    return path.ToArray(Allocator.TempJob);
                }
                finally
                {
                    path.Dispose();
                }
            }
        }
    }
}