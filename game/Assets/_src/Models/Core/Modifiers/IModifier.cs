using System;
using System.Runtime.InteropServices;
using Unity.Entities;

namespace Game.Model.Stats
{
    public readonly partial struct ModifiersAspect : IAspect
    {
        public readonly Entity Self;

        public readonly DynamicBuffer<Modifier> Items;
    }
    
    public interface IModifier
    {
        public delegate void Execute(TimeSpan delta, ref StatValue stat);
        
        void Estimation(TimeSpan delta, ref StatValue stat);
        void Attach(DynamicBuffer<Modifier> buff);
        void Dettach(DynamicBuffer<Modifier> buff);
    }

    public struct ModifiersInfo : IComponentData
    {
        public int Count;
    }

    public struct Modifier : IBufferElementData
    {
        public TypeIndex TypeIndex;
        public int TypeID;
        public int ID;
        Unity.Burst.FunctionPointer<IModifier.Execute> m_Method;

        public Modifier(IModifier.Execute estimation, Enum stat, TypeIndex typeIndex)
        {
            TypeID = stat.GetType().GetHashCode();
            ID = stat.GetHashCode();
            m_Method = new Unity.Burst.FunctionPointer<IModifier.Execute>(Marshal.GetFunctionPointerForDelegate(estimation));
            TypeIndex = typeIndex;
        }

        private static bool Find(DynamicBuffer<Modifier> buff, Modifier modifier, out int index)
        {
            index = 0;
            foreach (var iter in buff)
            {
                if (iter.TypeIndex == modifier.TypeIndex)
                    return true;
                index++;
            }
            return false;
        }

        public static void AddModifier<T>(DynamicBuffer<Modifier> buff, T modifier, Enum stat, out int id)
            where T : IModifier
        {
            var mod = new Modifier(modifier.Estimation, stat, TypeManager.GetTypeIndex(typeof(T)))
            {
            };

            if (Find(buff, mod, out id))
                buff[id] = mod;
            else
                id = buff.Add(mod);
        }

        public static void DelModifier(DynamicBuffer<Modifier> buff, int id)
        {
            buff.RemoveAt(id);
        }
    }
}