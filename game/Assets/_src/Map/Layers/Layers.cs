using System;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Common.Defs;

namespace Game.Model.Worlds
{
    public partial struct Map
    {
        public partial struct Layers
        {
            private static readonly Dictionary<TypeIndex, LayerInfo> m_Layers = new Dictionary<TypeIndex, LayerInfo>();

            delegate void DInit(ref SystemState systemState, Aspect aspect);
            delegate void DUpdate(ref SystemState systemState);
            delegate Entity DGetObject(Aspect aspect, int2 pos);
            delegate void DSetObject(Aspect aspect, int2 pos, Entity entity);

            public static void AddLayer<T>(Entity entity, IDefineableContext context)
                where T : unmanaged, ILayer
            {
                var type = typeof(T);
                var idx = TypeManager.GetTypeIndex(type);
                if (!m_Layers.ContainsKey(idx))
                    m_Layers.Add(idx, new LayerInfo(type));
                context.AddBuffer<T>(entity);
            }

            public static void Init(ref SystemState systemState, Aspect aspect)
            {
                foreach (var iter in m_Layers.Values)
                    iter.Init(ref systemState, aspect);
            }

            public static void Update(ref SystemState systemState)
            {
                foreach (var iter in m_Layers.Values)
                    iter.Update(ref systemState);
            }

            public static void SetObject(Aspect aspect, TypeIndex layerType, int2 pos, Entity entity)
            {
                if (!m_Layers.TryGetValue(layerType, out LayerInfo info))
                    throw new ArgumentException("Can`t find layer", TypeManager.GetTypeInfo(layerType).Type.FullName);
                info.SetObject(aspect, pos, entity);
            }

            public static bool TryGetObject(Aspect aspect, TypeIndex layerType, int2 pos, out Entity entity)
            {
                entity = Entity.Null;
                if (!m_Layers.TryGetValue(layerType, out LayerInfo info)) return false;
                entity = info.GetObject(aspect, pos);
                return entity != Entity.Null;
            }

            public static Entity GetObject(Aspect aspect, TypeIndex layerType, int2 pos)
            {
                if (!m_Layers.TryGetValue(layerType, out LayerInfo info))
                    throw new ArgumentException("Can`t find layer", TypeManager.GetTypeInfo(layerType).Type.FullName);
                return info.GetObject(aspect, pos);
            }

            private readonly struct LayerInfo
            {
                private readonly Type m_Type;
                private readonly DInit m_Init;
                private readonly DUpdate m_Update;
                private readonly DGetObject m_GetObject;
                private readonly DSetObject m_SetObject;
                
                public LayerInfo(Type type)
                {
                    m_Type = typeof(Layer<>).MakeGenericType(type);
                    m_Init = m_Type.GetDelegate<DInit>(nameof(Layer<Prototype>.Init), ReflectionHelper.STATIC);
                    m_Update = m_Type.GetDelegate<DUpdate>(nameof(Layer<Prototype>.Update), ReflectionHelper.STATIC);
                    m_GetObject = m_Type.GetDelegate<DGetObject>(nameof(Layer<Prototype>.GetObject), ReflectionHelper.STATIC);
                    m_SetObject = m_Type.GetDelegate<DSetObject>(nameof(Layer<Prototype>.SetObject), ReflectionHelper.STATIC);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Init(ref SystemState systemState, Aspect aspect) => m_Init.Invoke(ref systemState, aspect);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Update(ref SystemState systemState) => m_Update.Invoke(ref systemState);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void SetObject(Aspect aspect, int2 pos, Entity entity) => m_SetObject.Invoke(aspect, pos, entity);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public Entity GetObject(Aspect aspect, int2 pos) => m_GetObject.Invoke(aspect, pos);

                private struct Prototype : ILayer
                {
                    public Entity Entity { get; set; }
                }

            }

            readonly struct Layer<T>
                where T : unmanaged, ILayer
            {
                [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
                private static BufferLookup<T> m_Lookup;

                public static void Add()
                {
                    var idx = TypeManager.GetTypeIndex<T>();
                    m_Layers.Add(idx, new LayerInfo(typeof(Layer<T>)));
                }

                public static void Init(ref SystemState systemState, Aspect aspect)
                {
                    m_Lookup = systemState.GetBufferLookup<T>();
                    m_Lookup[aspect.Self].Resize(aspect.Value.Length, Unity.Collections.NativeArrayOptions.ClearMemory);
                }

                public static void Update(ref SystemState systemState)
                {
                    m_Lookup.Update(ref systemState);
                }

                public static void SetObject(Aspect aspect, int2 pos, Entity entity)
                {
                    var idx = aspect.Value.At(pos);
                    m_Lookup[aspect.Self].ElementAt(idx).Entity = entity;
                }

                public static Entity GetObject(Aspect aspect, int2 pos)
                {
                    var idx = aspect.Value.At(pos);
                    return m_Lookup[aspect.Self].ElementAt(idx).Entity;
                }

                public Layer(Entity entity)
                {
                    Entity = entity;
                }
                public readonly Entity Entity;
                public static implicit operator Layer<T>(Entity value) => new Layer<T>(value);
            }
        }
    }
}
