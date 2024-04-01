using System;
using System.Collections.Generic;
using Unity.Burst;

using UnityEngine;

namespace Game.Core
{
    public readonly partial struct CustomHandle
    {
        private readonly struct Manager
        {
            public static CustomHandle GetHandle<T>()
            {
                return SharedCustomHandle<T>.Get();
            }

            public static string GetName(CustomHandle value)
            {
                return m_Names[value];
            }

            private struct CustomHandleManagerKeyContext { }

            private struct SharedCustomHandle<TComponent>
            {
                public static ref CustomHandle Get()
                {
                    if (m_Ref.Data.m_ID == 0)
                    {
                        Registry(typeof(TComponent));
                    }
                    return ref m_Ref.Data;
                }
                static readonly SharedStatic<CustomHandle> m_Ref = SharedStatic<CustomHandle>.GetOrCreate<CustomHandleManagerKeyContext, TComponent>();
            }

            private struct SharedCustomHandle
            {
                public static ref CustomHandle Get(Type componentType)
                {
                    return ref SharedStatic<CustomHandle>.GetOrCreate(typeof(CustomHandleManagerKeyContext), componentType).Data;
                }
                public static void Set(Type componentType, CustomHandle value)
                {
                    ref var data = ref SharedStatic<CustomHandle>.GetOrCreate(typeof(CustomHandleManagerKeyContext), componentType).Data;
                    data = value;
                }
            }

            public static void Registry(Type type)
            {
                var name = $"{type}";
                var stringId = $"{type.FullName}";
                var handle = new CustomHandle(stringId.GetHashCode());
                SharedCustomHandle.Set(type, handle);
                try
                {
                    m_Names.Add(handle, name);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Game.Core.CustomHandle] {stringId} : {e}");
                }
            }

            private static readonly Dictionary<CustomHandle, string> m_Names = new ();
        }
    }
}