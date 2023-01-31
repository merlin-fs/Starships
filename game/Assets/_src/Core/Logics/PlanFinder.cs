using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public partial struct PlanFinder
        {
            public static NativeArray<LogicHandle> Execute(LogicHandle source, States goal,
                LogicDef logic,
                NativeArray<GoapAction> availableActions,
                States worldStates)
            {
                var finder = new PlanFinder()
                {
                };
                var path = finder.Search(source, goal, logic, availableActions, worldStates);
                return path;
            }

            struct Node: IEquatable<Node>
            {
                public LogicHandle Handle;
                public float Cost;

                public static Node Create(LogicHandle handle, float cost = 1f)
                {
                    Node result = default;
                    result.Handle = handle;
                    result.Cost = cost;
                    return result;
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

            private NativeArray<LogicHandle> Search(LogicHandle source, States goal,
                LogicDef logic,
                NativeArray<GoapAction> availableActions, States worldStates)
            {

                var queue = new PathFinder.SortedNativeHash<Node>(100, Allocator.TempJob,
                    (Node i1, Node i2) =>
                    {
                        float result = i1.Cost - i2.Cost;
                        return Math.Sign(result);
                    }
                );

                

                foreach (var iter in goal.GetReadOnly())
                {
                    var parts = logic.GetActionsFromGoal(iter.Key);
                    
                    foreach (var part in parts)
                    {
                        UnityEngine.Debug.Log(part);
                    }


                    /*
                    var node = Node.Create(iter.Handle, iter.Cost);
                    if (queue.Contains(node))
                        continue;

                    var tool = iter.GetGoalTools();
                    if (tool.LeadsToGoal(goal))
                    {
                        tool.RemoveEffect(goal);
                        tool.ApplyPreconditionsWithOutWorld(goal, worldStates);

                        queue.Push(node);
                    }
                    */
                }
                return default;
            }
        }
    }
}