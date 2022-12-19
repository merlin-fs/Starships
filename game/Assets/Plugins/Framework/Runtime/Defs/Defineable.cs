using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using UnityEngine;

namespace Common.Defs
{
    /*
    public struct Defineable<T> : IDefineable<T>
        where T : unmanaged, IComponentData
    {
        private static readonly ConstructorInfo m_Create = typeof(T).GetConstructor(new Type[] { typeof(IntPtr) });

        public T Value;
        public IComponentData GetValue() => Value;
        public unsafe Defineable(IntPtr ptr)
        {
            Value = (T)m_Create.Invoke(new object[] { ptr });
        }
    }
    */
}