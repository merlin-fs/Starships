using System;
using System.Collections.Generic;
using System.Linq;
using Common.Core;
using Common.Defs;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEditor;
#if UNITY_EDITOR         
using UnityEditor.Animations;
#endif
using UnityEngine;

namespace Game.Core.Animations
{
    public class AnimationCurvePosition
    {
        public AnimationCurve x;
        public AnimationCurve y;
        public AnimationCurve z;

        public void SetValue(string property, AnimationCurve curve)
        {
            switch (property)
            {
                case "m_LocalPosition.x": 
                    x = curve;
                    break;
                case "m_LocalPosition.y": 
                    y = curve;
                    break;
                case "m_LocalPosition.z": 
                    z = curve;
                    break;
            }
        }
    }

    public class AnimationCurveRotation
    {
        public AnimationCurve x;
        public AnimationCurve y;
        public AnimationCurve z;
        public AnimationCurve w;

        public void SetValue(string property, AnimationCurve curve)
        {
            switch (property)
            {
                case "m_LocalRotation.x":
                case "localEulerAnglesRaw.x":
                    x = curve;
                    break;
                case "m_LocalRotation.y": 
                case "localEulerAnglesRaw.y":
                    y = curve;
                    break;
                case "m_LocalRotation.z":
                case "localEulerAnglesRaw.z":
                    z = curve;
                    break;
                case "m_LocalRotation.w": 
                    w = curve;
                    break;
            }
        }
    }

    public class EntityClip
    {
        private readonly Dictionary<int, AnimationCurvePosition[]> m_Positions = new Dictionary<int, AnimationCurvePosition[]>();
        private readonly Dictionary<int, AnimationCurveRotation[]> m_Rotations = new Dictionary<int, AnimationCurveRotation[]>();
       
        public float Length { get; }
        public bool Loop { get; }
        
        public EntityClip(float length, bool loop)
        {
            Length = length;
            Loop = loop;
        }
        
        public void SetPosition(int threadId, int boneId, float t, ref float3 value)
        {
            if (!m_Positions.TryGetValue(boneId, out AnimationCurvePosition[] curves) || curves[threadId].x == null)
                return;
            var curve = curves[threadId];
            value = new float3(curve.x.Evaluate(t), curve.y.Evaluate(t), curve.z.Evaluate(t));
        }

        public void SetRotation(int threadId, int boneId, float t, ref quaternion value)
        {
            if (!m_Rotations.TryGetValue(boneId, out AnimationCurveRotation[] curves) || curves[threadId].x == null)
                return;

            var curve = curves[threadId];
            if (curve.w != null)
            {
                value = new quaternion(curve.x.Evaluate(t), curve.y.Evaluate(t), curve.x.Evaluate(t), curve.w.Evaluate(t));
            }
            else{
                var rotate = new float3(math.radians(curve.x.Evaluate(t)), math.radians(curve.y.Evaluate(t)), math.radians(curve.z.Evaluate(t)));
                value = quaternion.Euler(rotate);
            }
        }

        public void AddPosition(int boneId, string property, AnimationCurve curve)
        {
            if (!m_Positions.TryGetValue(boneId, out var value))
            {
                value = new AnimationCurvePosition[Animation.ArrayLength];
                for (int i = 0; i < Animation.ArrayLength; i++)
                    value[i] = new AnimationCurvePosition();
                m_Positions.Add(boneId, value);
            }

            for (int i = 0; i < Animation.ArrayLength; i++)
                value[i].SetValue(property, new AnimationCurve(curve.keys));
                
        }

        public void AddRotation(int boneId, string property, AnimationCurve curve)
        {
            if (!m_Rotations.TryGetValue(boneId, out var value))
            {
                value = new AnimationCurveRotation[Animation.ArrayLength];
                for (int i = 0; i < Animation.ArrayLength; i++)
                    value[i] = new AnimationCurveRotation();
                m_Rotations.Add(boneId, value);
            }
            for (int i = 0; i < Animation.ArrayLength; i++)
                value[i].SetValue(property, new AnimationCurve(curve.keys));
        }
    }

    public class EntityAnimatorConfig : ScriptableIdentifiable, IInitiated
    {
        public const string ROOT_NAME = "_root_";
        
        [HideInInspector]
        [SerializeField] private AnimatorClipItem [] m_ClipItems;
        [HideInInspector]
        [SerializeField] private AnimatorItem [] m_Items;
#if UNITY_EDITOR         
        [SerializeField] private AnimatorController m_AnimatorController;
#endif        

        private readonly Dictionary<ObjectID, EntityClip> m_Clips = new Dictionary<ObjectID, EntityClip>();

        public EntityClip GetClip(ObjectID id)
        {
            return m_Clips.TryGetValue(id, out var clip) 
                ? clip 
                : null;
        }

        private void AddClip(ObjectID id, float length, bool loop)
        {
            var clip = new EntityClip(length, loop);
            m_Clips.Add(id, clip);
        }

        void IInitiated.Init()
        {
            BuildHash();
        }
        
        void BuildHash()
        {
            foreach (var iter in m_ClipItems)
                AddClip(iter.ID, iter.Length, iter.Loop);
            
            foreach (var iter in m_Items)
            {
                var clip = GetClip(iter.ID);
                clip.AddPosition(iter.HashCode, iter.PropertyName, iter.Curve);
                clip.AddRotation(iter.HashCode, iter.PropertyName, iter.Curve);
            }
        }
        
#if UNITY_EDITOR         
        public override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
            m_Items = new AnimatorItem[]{};
            if (!m_AnimatorController) return;

            
            m_ClipItems = m_AnimatorController.animationClips
                .Select(iter => new AnimatorClipItem 
                {
                    Length = iter.length,
                    Loop = iter.isLooping,
                    ID = iter.name,
                })
                .ToArray();
            
            foreach (var iter in m_AnimatorController.animationClips.Where(clip => clip!= null))
            {
                var curves = AnimationUtility.GetCurveBindings(iter)
                    .GroupBy(curve => curve.path)
                    .SelectMany(group => group)
                    .Select(item => new AnimatorItem
                    {
                        ID = iter.name,
                        Curve = AnimationUtility.GetEditorCurve(iter, item),
                        PropertyName = item.propertyName,
                        HashCode = UnityEngine.Animator.StringToHash(string.IsNullOrEmpty(item.path) ? ROOT_NAME : item.path),
                    });
                m_Items = m_Items.Union(curves).ToArray();
            }
        }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
        }
#endif        
        [Serializable]
        private struct AnimatorClipItem
        {
            public ObjectID ID;
            public float Length;
            public bool Loop;
        }

        [Serializable]
        private struct AnimatorItem
        {
            public ObjectID ID;
            public AnimationCurve Curve;
            public string PropertyName;
            public int HashCode;
        }
    }
}