using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Unity.Collections
{
    public static partial class NativeCollections
    {
        public static unsafe void AddRangeNative<T>(this List<T> list, void* arrayBuffer, int length)
            where T : struct
        {
            if (length == 0)
            {
                return;
            }

            var index = list.Count;
            var newLength = index + length;

            // Resize our list if we require
            if (list.Capacity < newLength)
            {
                list.Capacity = newLength;
            }

            var items = NoAllocHelpers.ExtractArrayFromListT(list);
            var size = UnsafeUtility.SizeOf<T>();

            // Get the pointer to the end of the list
            var bufferStart = (IntPtr)UnsafeUtility.AddressOf(ref items[0]);
            var buffer = (byte*)(bufferStart + size * index);

            UnsafeUtility.MemCpy(buffer, arrayBuffer, length * (long)size);

            NoAllocHelpers.ResizeList(list, newLength);
        }
    }
    
    static class NoAllocHelpers
    {
        private static readonly Func<object, Array> ExtractArrayFromListTDelegates;
        private static readonly Action<object, int> ResizeListDelegates;

        static NoAllocHelpers()
        {
            ExtractArrayFromListTDelegates = GetExtractArrayFromListTDelegates();
            ResizeListDelegates = GetResizeListDelegates();
        }

        internal static T[] ExtractArrayFromListT<T>(List<T> list)
        {
            return (T[])ExtractArrayFromListTDelegates.Invoke(list);
        }

        internal static void ResizeList<T>(List<T> list, int size)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (size < 0 || size > list.Capacity)
            {
                throw new ArgumentException("Invalid size to resize.", nameof(list));
            }

            if (size == list.Count)
            {
                return;
            }

            ResizeListDelegates.Invoke(list, size);
        }

        private static Func<object, Array> GetExtractArrayFromListTDelegates()
        {
            var ass = Assembly.GetAssembly(typeof(Mesh)); // any class in UnityEngine
            //UnityEngine.NoAllocHelpers
            var type = ass.GetType("UnityEngine.NoAllocHelpers");

            var methodInfo = type.GetMethod("ExtractArrayFromList", BindingFlags.Static | BindingFlags.Public);

            if (methodInfo == null)
            {
                throw new("ExtractArrayFromList signature changed.");
            }

            return (Func<object, Array>)methodInfo.CreateDelegate(typeof(Func<object, Array>));
        }

        private static Action<object, int> GetResizeListDelegates()
        {
            var ass = Assembly.GetAssembly(typeof(Mesh)); // any class in UnityEngine
            var type = ass.GetType("UnityEngine.NoAllocHelpers");

            var methodInfo = type.GetMethod("Internal_ResizeList", BindingFlags.Static | BindingFlags.NonPublic);

            if (methodInfo == null)
            {
                throw new("Internal_ResizeList signature changed.");
            }

            return (Action<object, int>)methodInfo.CreateDelegate(typeof(Action<object, int>));
        }
    }
}