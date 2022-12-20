using System;
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
            stat.Value.Reset();
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

        public void DelModifier(uint uid)
        {
            var items = Items;
            var id = FindFreeItem();
            if (id < 0)
                return;
            
            items[id] = new Modifier() { Active = false };

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
        public uint UID;
        private readonly Unity.Burst.FunctionPointer<IModifier.Execute> m_Method;

        private Modifier(IntPtr method, uint uid, Enum stat)
        {
            UID = uid;
            StatID = new int2(stat.GetType().GetHashCode(), stat.GetHashCode()).GetHashCode();
            //m_Method = new Unity.Burst.FunctionPointer<IModifier.Execute>(Marshal.GetFunctionPointerForDelegate(estimation));
            m_Method = new Unity.Burst.FunctionPointer<IModifier.Execute>(method);
            Active = true;
        }

        public static Modifier Create<T>(ref T modifier, uint uid, Enum stat)
            where T : IModifier
        {
            return new Modifier(Marshal.GetFunctionPointerForDelegate<IModifier.Execute>(modifier.Estimation), uid, stat);
        }

        public void Estimation(Entity entity, ref StatValue stat, float delta)
        {
            try
            {
                var method = Marshal.GetDelegateForFunctionPointer<IModifier.Execute>(m_Method.Value);
                if (method.Target == null)
                    return; 
                UnityEngine.Debug.Log(method.Target.ToString());
                m_Method.Invoke(entity, ref stat, delta);
            }
            catch (Exception e)
            { 
                UnityEngine.Debug.LogException(e);
            }
            
        }


        сделать Task, который будет возвращать ID после установления модификатора
        public static unsafe uint AddModifier<T>(Entity entity, ref T modifier, Enum statType)
            where T : struct, IModifier
        {
            uint uid = (uint)new IntPtr(UnsafeUtility.AddressOf<T>(ref modifier)).ToInt32();
            ModifiersSystem.Instance.AddModifier(entity, ref modifier, uid, statType);
            return uid;
        }

        сделать Task, который будет возвращать после удаления модификатора
        public static void DelModifier(Entity entity, uint uid)
        {
            ModifiersSystem.Instance.DelModifier(entity, uid);
        }
    }
}