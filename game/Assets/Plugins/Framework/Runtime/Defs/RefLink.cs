using System;
using System.Runtime.InteropServices;

namespace Common.Defs
{
    public struct RefLink<T>
    {
        private GCHandle m_RefHandle;
        public T Value => (T)m_RefHandle.Target;
        public RefLink(GCHandle handle) => m_RefHandle = handle;
    }

}