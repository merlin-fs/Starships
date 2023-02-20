using System;
using Unity.Entities;

namespace Unity.Transforms
{
    public static class TransformsExt 
    {
        public static void ChildrenForEach(ref this BufferLookup<Child> children, Entity root, Action<Entity> action)
        {
            if (children.HasBuffer(root))
                foreach (var child in children[root])
                {
                    action.Invoke(child.Value);
                    if (children.HasBuffer(child.Value))
                        ChildrenForEach(ref children, child.Value, action);
                }
        }

        public static void LinkedForEach(ref this DynamicBuffer<LinkedEntityGroup> group, Entity root, Action<Entity> action)
        {
            foreach (var child in group)
            {
                action.Invoke(child.Value);
            }
        }
    }
}

