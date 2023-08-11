using System.Collections;
using System.Collections.Generic;

using Unity.Collections.LowLevel.Unsafe;

using UnityEngine;

namespace Unity.Collections
{
    public interface IRefComparer<T>
        where T : unmanaged
    {
        int Compare(ref T x, ref T y);
    }
    
    public static class ArrayFindIndex
    {
        public unsafe static int Find<T, U>(this NativeArray<T> array, ref T value, U comp)
            where T : unmanaged
            where U : IRefComparer<T>
        {
            return Find((T*)array.GetUnsafeReadOnlyPtr(), array.Length, ref value, comp);
        }

        public unsafe static int Find<T, U>(T* ptr, int length, ref T value, U comparer)
            where T : unmanaged
            where U : IRefComparer<T>
        {
            {
                var offset = 0;

                for (var l = length; l != 0; l >>= 1)
                {
                    var idx = offset + (l >> 1);
                    var curr = ptr[idx];
                    var r = comparer.Compare(ref value, ref curr);
                    switch (r)
                    {
                        case 0:
                            return idx;
                        case > 0:
                            offset = idx + 1;
                            --l;
                            break;
                    }
                }
                return offset >= length 
                    ? ~offset 
                    : offset;
            }
        }
    }
}