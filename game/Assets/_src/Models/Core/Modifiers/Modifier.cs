using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model.Stats
{
    public unsafe struct Modifier : IBufferElementData, IEquatable<Modifier>
    {
        private static readonly MethodInfo m_Method = typeof(IModifier).GetMethod(nameof(IModifier.Estimation));

        public bool Active;
        public int StatID;
        public TypeIndex TypeIndex;
        
        [NativeDisableUnsafePtrRestriction]
        private readonly void* m_ModifierPtr;

        public ulong UID => (ulong)m_ModifierPtr;

        private Modifier(void* ptr, Enum stat)
        {
            StatID = new int2(stat.GetType().GetHashCode(), stat.GetHashCode()).GetHashCode();
            m_ModifierPtr = ptr;
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

        public void Estimation(Entity entity, ref StatValue stat, float delta)
        {
            try
            {
                var obj = (IModifier)Marshal.PtrToStructure(new IntPtr(m_ModifierPtr), TypeManager.GetTypeInfo(TypeIndex).Type);
                obj.Estimation(entity, ref stat, delta);
                /*
                object obj = Marshal.PtrToStructure(new IntPtr(m_ModifierPtr), TypeManager.GetTypeInfo(TypeIndex).Type);
                IModifier.Execute estimation = (IModifier.Execute)Delegate.CreateDelegate(typeof(IModifier.Execute), obj, m_Method);
                estimation.Invoke(entity, ref stat, delta);
                */
                /*
                var args = new object[] { entity, stat, delta };
                (IModifier.Execute)m_Method.Invoke(obj, args);
                stat = (StatValue)args[1];
                */
            }
            catch (Exception e)
            { 
                UnityEngine.Debug.LogException(e);
            }
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