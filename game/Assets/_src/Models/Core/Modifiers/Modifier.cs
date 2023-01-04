using System;
using System.Reflection;
using System.Runtime.InteropServices;

using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Properties;

using UnityEngine;

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
            var ptr = UnsafeUtility.AddressOf<T>(ref modifier);
            return new Modifier(ptr, stat)
            {
                TypeIndex = TypeManager.GetTypeIndex<T>(),
            };
        }

        [BurstDiscard]
        public void Estimation(Entity entity, ref Stat stat, float delta)
        {
            var obj = (IModifier)Marshal.PtrToStructure(new IntPtr((void*)m_ModifierPtr), TypeManager.GetTypeInfo(TypeIndex).Type);
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
    }
}