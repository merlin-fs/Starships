using System;
using System.Reflection;
using System.Runtime.InteropServices;

using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Properties;

using UnityEngine;

using static UnityEditor.Progress;

namespace Game.Model.Stats
{
    [Serializable]
    [WriteGroup(typeof(Stat))]
    public unsafe struct Modifier : IBufferElementData, IEquatable<Modifier>
    {
        private static readonly MethodInfo m_Method = typeof(IModifier).GetMethod(nameof(IModifier.Estimation));

        public bool Active;
        [HideInInspector]
        public int StatID;
        [HideInInspector]
        public TypeIndex TypeIndex;
#if DEBUG
        [CreateProperty]
        public string StatName => StatID != 0 ? Stats.Stat.GetName(StatID) : "null";
#endif
        //[NativeDisableUnsafePtrRestriction]
        private readonly ulong m_ModifierPtr;
        [HideInInspector]
        public ulong UID => (ulong)m_ModifierPtr;

        private Modifier(void* ptr, Enum stat)
        {
            StatID = new int2(stat.GetType().GetHashCode(), stat.GetHashCode()).GetHashCode();
            m_ModifierPtr = (ulong)ptr;
            Active = true;
            TypeIndex = 0;
        }

        bool IEquatable<Modifier>.Equals(Modifier other)
        {
            return (m_ModifierPtr == other.m_ModifierPtr);
        }

        public static Modifier Create<T>(ref T modifier, Enum stat)
            where T : struct, IModifier
        {
            UnsafeUtility.PinGCObjectAndGetAddress(modifier, out ulong handle);
            return new Modifier((void*)handle, stat)
            {
                TypeIndex = TypeManager.GetTypeIndex<T>(),
            };
        }

        public void Dispose()
        {
            UnsafeUtility.ReleaseGCObject(m_ModifierPtr);
        }

        [BurstDiscard]
        public void Estimation(Entity entity, ref Stat stat, float delta)
        {
            //var obj = (IModifier)Marshal.PtrToStructure(new IntPtr((void*)m_ModifierPtr), TypeManager.GetTypeInfo(TypeIndex).Type);
            var obj = (IModifier)GCHandle.FromIntPtr(new IntPtr((void*)m_ModifierPtr)).Target;
            obj.Estimation(entity, ref stat, delta);
        }

        public static ulong AddModifierAsync<T>(Entity entity, ref T modifier, Enum statType)
            where T : struct, IModifier
        {
            return ModifiersSystem.Instance.AddModifier(entity, ref modifier, statType);
        }

        public static void DelModifierAsync(Entity entity, ulong uid)
        {
            ModifiersSystem.Instance.DelModifier(entity, uid);
        }

        public static void Estimation(Entity entity, ref Stat stat, in DynamicBuffer<Modifier> items, float delta)
        {
            stat.Reset();
            foreach (var item in items)
                if (item.Active && item.StatID == stat.StatID)
                {
                    item.Estimation(entity, ref stat, delta);
                }
        }

        public static int AddModifier(Modifier modifier, ref DynamicBuffer<Modifier> items)
        {
            var id = FindFreeItem(items);
            if (id < 0)
                id = items.Add(modifier);
            else
                items[id] = modifier;

            return id;

            int FindFreeItem(DynamicBuffer<Modifier> items)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (!items[i].Active)
                        return i;
                }
                return -1;
            }
        }

        public static void DelModifier(ulong uid, ref DynamicBuffer<Modifier> items)
        {
            if (uid == 0)
                return;

            var id = FindFreeItem(items);
            if (id < 0)
                return;

            items[id].Dispose();
            items[id] = new Modifier() { Active = false };

            int FindFreeItem(DynamicBuffer<Modifier> items)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].UID == uid)
                        return i;
                }
                return -1;
            }
        }
    }
}