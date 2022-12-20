using System;
using System.Reflection;
using System.Runtime.InteropServices;

using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model.Stats
{
    public readonly partial struct ModifiersAspect : IAspect
    {
        public readonly Entity Self;

        public readonly DynamicBuffer<Modifier> Items;

        public void Estimation(ref Stat stat, float delta)
        {
            foreach (var item in Items)
            {
                if (item.Active && item.StatID == stat.StatID)
                {
                    item.Estimation(Self, ref stat.Value, delta);
                }
            }
        }

        public void AddModifier(Modifier modifier)
        {
            var items = Items;
            var id = FindFreeItem();
            if (id < 0)
                items.Add(modifier);
            else
                items[id] = modifier;

            int FindFreeItem()
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (!items[i].Active)
                        return i;
                }
                return -1;
            }
        }

        public void DelModifier(int uid)
        {
            var items = Items;
            var id = FindFreeItem();
            UnityEngine.Debug.Log($"DelModifier {id}");

            if (id < 0)
                return;
            
            items[id] = new Modifier(false);

            int FindFreeItem()
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

    public interface IModifier
    {
        public delegate void Execute(Entity entity, ref StatValue stat, float delta);
        
        void Estimation(Entity entity, ref StatValue stat, float delta);
        void Attach(Entity entity);
        void Dettach(Entity entity);
    }

    public struct Modifier : IBufferElementData
    {
        public bool Active;
        public int StatID;
        public int UID;
        private readonly Unity.Burst.FunctionPointer<IModifier.Execute> m_Method;

        public Modifier(IModifier.Execute estimation, int uid, Enum stat)
        {
            UID = uid;
            StatID = new int2(stat.GetType().GetHashCode(), stat.GetHashCode()).GetHashCode();
            m_Method = new Unity.Burst.FunctionPointer<IModifier.Execute>(Marshal.GetFunctionPointerForDelegate(estimation));
            Active = true;
        }

        public Modifier(bool _ = false)
        {
            UID = -1;
            StatID = -1;
            m_Method = default;
            Active = false;
        }

        public void Estimation(Entity entity, ref StatValue stat, float delta)
        {
            m_Method.Invoke(entity, ref stat, delta);
        }

        public static unsafe int AddModifier<T>(Entity entity, ref T modifier, Enum statType)
            where T : struct, IModifier
        {
            int uid = new IntPtr(UnsafeUtility.AddressOf<T>(ref modifier)).ToInt32();
            ModifiersSystem.Instance.AddModifier(entity, ref modifier, uid, statType);
            return uid;
        }

        public static void DelModifier(Entity entity, int uid)
        {
            ModifiersSystem.Instance.DelModifier(entity, uid);
        }
    }
}