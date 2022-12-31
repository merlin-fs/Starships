using System;
using Unity.Entities;

namespace Game.Model.Stats
{
    public readonly partial struct ModifiersAspect : IAspect
    {
        private readonly Entity m_Self;

        public readonly DynamicBuffer<Modifier> Items;

        public void Estimation(ref Stat stat, float delta)
        {
            stat.Value.Reset();
            foreach (var item in Items)
                if (item.Active && item.StatID == stat.StatID)
                {
                    item.Estimation(m_Self, ref stat.Value, delta);
                }
        }

        public int AddModifier(Modifier modifier)
        {
            var items = Items;
            var id = FindFreeItem();
            if (id < 0)
                items.Add(modifier);
            else
                items[id] = modifier;

            return id;

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

        public void DelModifier(ulong uid)
        {
            if (uid == 0)
                return;

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
}