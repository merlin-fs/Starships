using System;
using System.Runtime.InteropServices;
using Unity.Entities;

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
                if (item.Active && item.TypeID == stat.TypeID && item.ID == stat.ID)
                {
                    item.Estimation(Self, ref stat.Value, delta);
                }
            }
        }

        public void AddModifier<T>(T modifier, Enum statType, out int id)
            where T : IModifier
        {
            var items = Items;
            var mod = new Modifier(modifier.Estimation, statType)
            {
                Active = true,
            };
            id = FindFreeItem();
            if (id < 0)
                id = items.Add(mod);
            else
                items[id] = mod;
        }

        public void DelModifier(int id)
        {
            var items = Items;
            items[id] = new Modifier() { Active = false };
        }

        private int FindFreeItem()
        {
            for (int i = 0; i < Items.Length; i++)
            {
                if (!Items[i].Active)
                    return i;
            }
            return -1;
        }
    }

    public interface IModifier
    {
        public delegate void Execute(Entity entity, ref StatValue stat, float delta);
        
        void Estimation(Entity entity, ref StatValue stat, float delta);
        void Attach(Entity entity);
        void Dettach(Entity entity);
    }

    public struct ModifiersInfo : IComponentData
    {
        public int Count;
    }

    public struct Modifier : IBufferElementData
    {
        public bool Active;
        public int TypeID;
        public int ID;
        Unity.Burst.FunctionPointer<IModifier.Execute> m_Method;

        public Modifier(IModifier.Execute estimation, Enum stat)
        {
            TypeID = stat.GetType().GetHashCode();
            ID = stat.GetHashCode();
            m_Method = new Unity.Burst.FunctionPointer<IModifier.Execute>(Marshal.GetFunctionPointerForDelegate(estimation));
            Active = false;
        }

        public void Estimation(Entity entity, ref StatValue stat, float delta)
        {
            m_Method.Invoke(entity, ref stat, delta);
        }

        public static void AddModifier<T>(Entity entity, T modifier, Enum statType, out int id)
            where T : IModifier
        {
            var aspect = World.DefaultGameObjectInjectionWorld.EntityManager.GetAspect<ModifiersAspect>(entity);
            aspect.AddModifier(modifier, statType, out id);
        }

        public static void DelModifier(Entity entity, int id)
        {
            var aspect = World.DefaultGameObjectInjectionWorld.EntityManager.GetAspectRO<ModifiersAspect>(entity);
            aspect.DelModifier(id);
        }
    }
}