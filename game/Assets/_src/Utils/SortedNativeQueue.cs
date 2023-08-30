using System;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections
{
    public struct SortedNativeQueue<TKey>
        where TKey : unmanaged, IEquatable<TKey>
    {
        public delegate int Compare<T>(T x, T y) where T : struct;

        private readonly Compare<TKey> m_Comparer;
        private NativeList<TKey> m_Sorted;

        public SortedNativeQueue(int length, Allocator allocator, Compare<TKey> comparer)
        {
            m_Comparer = comparer;
            m_Sorted = new NativeList<TKey>(length, allocator);
        }

        private int Insert(NativeList<TKey> values, TKey key)
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

        private static unsafe void Delete(NativeList<TKey> values, int index)
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
}