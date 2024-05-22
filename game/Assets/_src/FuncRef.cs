using Unity.Collections.LowLevel.Unsafe;

using UnityEngine;

namespace System
{
    public delegate ref TResult FuncRef<TResult>();

    public unsafe readonly struct StructRef<T>
        where T : struct
    {
        readonly private byte* m_Data;
        public ref T Value => ref UnsafeUtility.AsRef<T>(m_Data); 
        public StructRef(ref T value)
        {
            m_Data = (byte*)UnsafeUtility.AddressOf<T>(ref value);
        }

        public StructRef(byte* data)
        {
            m_Data = data;
        }

        //public static explicit operator StructRef<T>(byte* value) => new StructRef<T>(value);
        public static implicit operator StructRef<T>(byte* value) => new StructRef<T>(value);
    }

    public readonly struct StructRef
    {
        public unsafe static StructRef<T> Get<T>(ref T value)
            where T : struct
        {
            return (byte*)UnsafeUtility.AddressOf<T>(ref value);
        }
        
    }
}
