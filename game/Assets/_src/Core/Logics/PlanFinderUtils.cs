using System;

using Game.Core;

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public partial struct PlanFinder
        {
            private static NativeHashMap<LogicActionHandle, Node>[] m_Costs;
            private static SortedNativeQueue<Node>[] m_Queue;
            private static NativeHashMap<LogicActionHandle, LogicActionHandle>[] m_Hierarchy;

            private static NativeHashMap<LogicActionHandle, Node> GetCosts(int threadIdx) => m_Costs[threadIdx];
            private static SortedNativeQueue<Node> GetQueue(int threadIdx) => m_Queue[threadIdx];
            private static NativeHashMap<LogicActionHandle, LogicActionHandle> GetHierarchy(int threadIdx) => m_Hierarchy[threadIdx];

            public static void Initialize()
            {
                var cpus = Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount + 2;
                m_Costs = new NativeHashMap<LogicActionHandle, Node>[cpus];
                for (int i = 0; i < m_Costs.Length; i++)
                    m_Costs[i] = new NativeHashMap<LogicActionHandle, Node>(100, Allocator.Persistent);

                m_Hierarchy = new NativeHashMap<LogicActionHandle, LogicActionHandle>[cpus];
                for (int i = 0; i < m_Hierarchy.Length; i++)
                    m_Hierarchy[i] = new NativeHashMap<LogicActionHandle, LogicActionHandle>(100, Allocator.Persistent);

                m_Queue = new SortedNativeQueue<Node>[cpus];
                for (int i = 0; i < m_Queue.Length; i++)
                    m_Queue[i] = new SortedNativeQueue<Node>(100, Allocator.Persistent,
                    (Node i1, Node i2) =>
                    {
                        float result = i1.HeuristicCost.Value - i2.HeuristicCost.Value;
                        return Math.Sign(result);
                    });
            }

            public static void InitFinder(int threadIdx)
            {

            }

            public static void FinishFinder(int threadIdx)
            {
                var costs = m_Costs[threadIdx];
                foreach(var iter in costs)
                    iter.Value.Dispose();
                costs.Clear();
                m_Queue[threadIdx].Clear();
                m_Hierarchy[threadIdx].Clear();
            }

            public static void Dispose()
            {
                Array.ForEach(m_Costs, iter => iter.Dispose());
                Array.ForEach(m_Hierarchy, iter => iter.Dispose());
                Array.ForEach(m_Queue, iter => iter.Dispose());
            }
        }
    }
}