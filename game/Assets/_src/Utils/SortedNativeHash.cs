using System;

namespace Unity.Collections
{
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