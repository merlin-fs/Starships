using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public partial struct PlanFinder
        {
            private static NativeHashMap<LogicHandle, Node>[] m_Costs;
            private static SortedQueue<Node>[] m_Queue;
            private static NativeHashMap<LogicHandle, LogicHandle>[] m_Hierarchy;

            private static NativeHashMap<LogicHandle, Node> GetCosts(int threadIdx) => m_Costs[threadIdx];
            private static SortedQueue<Node> GetQueue(int threadIdx) => m_Queue[threadIdx];
            private static NativeHashMap<LogicHandle, LogicHandle> GetHierarchy(int threadIdx) => m_Hierarchy[threadIdx];

            public static void Init()
            {
                var cpus = Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount + 1;
                m_Costs = new NativeHashMap<LogicHandle, Node>[cpus];
                Array.Fill(m_Costs, new NativeHashMap<LogicHandle, Node>(100, Allocator.Persistent));

                m_Hierarchy = new NativeHashMap<LogicHandle, LogicHandle>[cpus];
                Array.Fill(m_Hierarchy, new NativeHashMap<LogicHandle, LogicHandle>(100, Allocator.Persistent));

                m_Queue = new SortedQueue<Node>[cpus];
                Array.Fill(m_Queue, new SortedQueue<Node>(100, Allocator.Persistent,
                    (Node i1, Node i2) =>
                    {
                        float result = i1.HeuristicCost.Value - i2.HeuristicCost.Value;
                        return Math.Sign(result);
                    }));

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

            }

            public struct SortedQueue<TKey>
                where TKey : unmanaged, IEquatable<TKey>
            {
                public delegate int Compare<T>(T x, T y) where T : struct;
                private readonly Compare<TKey> m_Comparer;
                private NativeList<TKey> m_Sorted;

                public SortedQueue(int length, Allocator allocator, Compare<TKey> comparer)
                {
                    m_Comparer = comparer;
                    m_Sorted = new NativeList<TKey>(length, allocator);
                }

                int Insert(NativeList<TKey> values, TKey key)
                {
                    values.Length++;
                    bool found = false;
                    int i;
                    values[^1] = key;
                    for (i = values.Length - 1; i >= 0; i--)
                    {
                        var cmp = m_Comparer.Invoke(key, values[i]);
                        if (cmp < 0)
                        {
                            found = true;
                            values[i + 1] = values[i];
                        }
                        else if (found)
                        {
                            values[i + 1] = key;
                            break;
                        }
                    }
                    if (found && i == -1)
                        values[i + 1] = key;
                    return i + 1;
                }

                unsafe void Delete(NativeList<TKey> values, int index)
                {
                    var list = values.GetUnsafeList();
                    void* destination = (byte*)list->Ptr + (++index * sizeof(TKey));
                    UnsafeUtility.MemCpy(list->Ptr, destination, list->Length * sizeof(TKey));
                    values.Length--;
                }

                public bool Pop(out TKey value)
                {
                    if (m_Sorted.Length <= 0)
                    {
                        value = default;
                        return false;
                    }
                    value = m_Sorted[0];
                    Delete(m_Sorted, 0);
                    return true;
                }

                public void Push(TKey key)
                {
                    Insert(m_Sorted, key);
                }

                public int Count { get => m_Sorted.Length; }

                public void Clear()
                {
                    m_Sorted.Clear();
                }

                public void Dispose()
                {
                    m_Sorted.Dispose();
                }
            }
        
            public struct SortedNativeHash<TKey>
                where TKey : unmanaged, IEquatable<TKey>
            {
                public delegate int Compare<T>(T x, T y) where T : struct;
                private readonly Compare<TKey> m_Comparer;
                private NativeList<TKey> m_Sorted;
                private NativeHashSet<TKey> m_Values;

                public SortedNativeHash(int length, Allocator allocator, Compare<TKey> comparer)
                {
                    m_Comparer = comparer;
                    m_Sorted = new NativeList<TKey>(length, allocator);
                    m_Values = new NativeHashSet<TKey>(length, allocator);
                }

                public bool Contains(TKey value)
                {
                    return m_Values.Contains(value);
                }

                int Insert(NativeList<TKey> values, TKey key)
                {
                    values.Length++;
                    bool found = false;
                    int i;
                    values[^1] = key;
                    for (i = values.Length - 1; i >= 0; i--)
                    {
                        var cmp = m_Comparer.Invoke(key, values[i]);
                        if (cmp < 0)
                        {
                            found = true;
                            values[i + 1] = values[i];
                        }
                        else if (found)
                        {
                            values[i + 1] = key;
                            break;
                        }
                    }
                    if (found && i == -1)
                        values[i + 1] = key;
                    return i + 1;
                }

                void Delete(NativeList<TKey> values, int index)
                {
                    int i;
                    for (i = index; i < values.Length - 1; i++)
                        values[i] = values[i + 1];
                    values.Length--;
                }

                public bool Pop(out TKey value)
                {
                    if (m_Values.Count <= 0)
                    {
                        value = default;
                        return false;
                    }

                    value = m_Sorted[0];
                    Delete(m_Sorted, 0);
                    m_Values.Remove(value);
                    return true;
                }

                public void Push(TKey key)
                {
                    if (m_Values.Contains(key))
                        Delete(m_Sorted, m_Sorted.IndexOf(key));
                    else
                        m_Values.Add(key);
                    Insert(m_Sorted, key);
                }

                public int Count { get => m_Values.Count; }

                public void Clear()
                {
                    m_Sorted.Clear();
                    m_Values.Clear();
                }

                public void Dispose()
                {
                    m_Sorted.Dispose();
                    m_Values.Dispose();
                }
            }
        }
    }
}