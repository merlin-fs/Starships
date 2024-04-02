using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
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
            private static readonly Dictionary<TypeIndex, InternalLayerInfo> m_Layers = new Dictionary<TypeIndex, InternalLayerInfo>();

            public static void AddLayer<T>(Entity entity, IDefinableContext context)
                where T : unmanaged, ILayer
            {
                var type = typeof(T);
                var idx = TypeManager.GetTypeIndex(type);
                if (!m_Layers.ContainsKey(idx))
                    m_Layers.Add(idx, new InternalLayerInfo(type, idx, new DefaultValidator()));
                context.AddBuffer<T>(entity);
            }
            
            
            //TODO подумать над установкой валидатора через Reflection
            //как вариант, объявит интерфасе ILayerValidator<T> where T : ILayer
            //и через Reflection устанавливать при добавлении слоя (будет возможность добавлять несколько валидаторов)
            
            public static void AddLayer<T, TV>(Entity entity, IDefinableContext context)
                where T : unmanaged, ILayer
                where TV : ILayerValidator, new()
            {
                var type = typeof(T);
                var idx = TypeManager.GetTypeIndex(type);
                if (!m_Layers.ContainsKey(idx))
                    m_Layers.Add(idx, new InternalLayerInfo(type, idx, new TV()));
                context.AddBuffer<T>(entity);
            }

            private static void AddLayer<T>(Entity entity, ILayerValidator validator, IDefinableContext context)
                where T : unmanaged, ILayer
            {
                var type = typeof(T);
                var idx = TypeManager.GetTypeIndex(type);
                if (!m_Layers.ContainsKey(idx))
                    m_Layers.Add(idx, new InternalLayerInfo(type, idx, validator));
                context.AddBuffer<T>(entity);
            }

            public static IEnumerable<LayerInfo> Values => m_Layers.Values.Select(iter => iter.LayerInfo);
            
            public static void Initialize(ref SystemState systemState, Aspect aspect)
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
                if (!m_Layers.TryGetValue(layerType, out InternalLayerInfo info))
                    throw new ArgumentException("Can`t find layer", TypeManager.GetTypeInfo(layerType).Type.FullName);
                
                info.SetObject(aspect, pos, entity);
            }

            public static bool TryGetObject(Aspect aspect, TypeIndex layerType, int2 pos, out Entity entity)
            {
                entity = Entity.Null;
                if (!m_Layers.TryGetValue(layerType, out InternalLayerInfo info)) return false;
                entity = info.GetObject(aspect, pos);
                return entity != Entity.Null;
            }

            public static Entity GetObject(Aspect aspect, TypeIndex layerType, int2 pos)
            {
                if (!m_Layers.TryGetValue(layerType, out InternalLayerInfo info))
                    throw new ArgumentException("Can`t find layer", TypeManager.GetTypeInfo(layerType).Type.FullName);
                return info.GetObject(aspect, pos);
            }

            private struct DefaultValidator : ILayerValidator
            {
                public bool CanPlace(Aspect aspect, int2 pos, Entity entity) => true;
            }

            public readonly struct LayerInfo
            {
                public Type Type { get; }
                public Type SelfType { get; }
                public TypeIndex TypeIndex { get; }

                internal LayerInfo(Type type, Type selfType, TypeIndex typeIndex)
                {
                    Type = type;
                    SelfType = selfType;
                    TypeIndex = typeIndex;
                }
            }
            
            private readonly struct InternalLayerInfo
            {
                delegate void DInit(ref SystemState systemState, Aspect aspect);
                delegate void DUpdate(ref SystemState systemState);
                delegate Entity DGetObject(Aspect aspect, int2 pos);
                delegate void DSetObject(Aspect aspect, int2 pos, Entity entity);
                
                public LayerInfo LayerInfo => m_LayerInfo;

                private readonly DInit m_Init;
                private readonly DUpdate m_Update;
                private readonly DGetObject m_GetObject;
                private readonly DSetObject m_SetObject;
                private readonly ILayerValidator m_Validator;
                private readonly LayerInfo m_LayerInfo;
                
                public InternalLayerInfo(Type type, TypeIndex idx, ILayerValidator validator)
                {
                    m_Validator = validator;
                    Type genericType = typeof(Layer<>).MakeGenericType(type);
                    m_LayerInfo = new LayerInfo(genericType, type, idx);
                    m_Init = genericType.GetDelegate<DInit>(nameof(Layer<Prototype>.Init), ReflectionHelper.STATIC);
                    m_Update = genericType.GetDelegate<DUpdate>(nameof(Layer<Prototype>.Update), ReflectionHelper.STATIC);
                    m_GetObject = genericType.GetDelegate<DGetObject>(nameof(Layer<Prototype>.GetObject), ReflectionHelper.STATIC);
                    m_SetObject = genericType.GetDelegate<DSetObject>(nameof(Layer<Prototype>.SetObject), ReflectionHelper.STATIC);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Init(ref SystemState systemState, Aspect aspect) => m_Init.Invoke(ref systemState, aspect);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Update(ref SystemState systemState) => m_Update.Invoke(ref systemState);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void SetObject(Aspect aspect, int2 pos, Entity entity)
                {
                    if (m_Validator.CanPlace(aspect, pos, entity))
                        m_SetObject.Invoke(aspect, pos, entity);
                } 

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public Entity GetObject(Aspect aspect, int2 pos) => m_GetObject.Invoke(aspect, pos);

                private struct Prototype : ILayer
                {
                    public Entity Entity { get; set; }
                }

            }

            private readonly struct Layer<T>
                where T : unmanaged, ILayer
            {
                [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
                private static BufferLookup<T> m_Lookup;

                public static void Init(ref SystemState systemState, Aspect aspect)
                {
                    m_Lookup = systemState.GetBufferLookup<T>();
                    m_Lookup[aspect.Self].Resize(aspect.Value.Length, NativeArrayOptions.ClearMemory);
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

                /*
                public Layer(Entity entity)
                {
                    Entity = entity;
                }
                
                public readonly Entity Entity;
                
                public static implicit operator Layer<T>(Entity value) => new Layer<T>(value);
                */
            }
        }
    }
}
