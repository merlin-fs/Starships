using System;
using System.Runtime.InteropServices;

namespace Common.Defs
{
    public struct RefLink<T>
    {
        private GCHandle m_RefHandle;
        public T Value => (T)m_RefHandle.Target;
        public RefLink(GCHandle handle) => m_RefHandle = handle;
        public static RefLink<T> Copy<TT>(RefLink<TT> link) => new RefLink<T>(link.m_RefHandle);
        public static RefLink<T> From(T value) => new RefLink<T>(GCHandle.Alloc(value, GCHandleType.Pinned));
        public static void Free(RefLink<T> value) => value.m_RefHandle.Free();
    }
}