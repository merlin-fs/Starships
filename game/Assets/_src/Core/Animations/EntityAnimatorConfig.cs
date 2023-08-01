using System;
using System.Collections.Generic;
using System.Linq;
using Common.Core;
using Common.Defs;

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
                    x = curve;
                    break;
                case "m_LocalRotation.y": 
                    y = curve;
                    break;
                case "m_LocalRotation.z": 
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
        private Dictionary<int, AnimationCurvePosition> m_Positions = new Dictionary<int, AnimationCurvePosition>();
        private Dictionary<int, AnimationCurveRotation> m_Rotations = new Dictionary<int, AnimationCurveRotation>();

        public bool GetPosition(int boneId, float t, out float3 value)
        {
            value = float3.zero;
            if (!m_Positions.TryGetValue(boneId, out AnimationCurvePosition curve) || curve.x == null)
                return false;
            value = new float3(curve.x.Evaluate(t), curve.y.Evaluate(t), curve.z.Evaluate(t));
            return true;
        }

        public bool GetRotation(int boneId, float t, out quaternion value)
        {
            value = quaternion.identity;
            if (!m_Rotations.TryGetValue(boneId, out AnimationCurveRotation curve) || curve.x == null)
                return false;
            value = new quaternion(curve.x.Evaluate(t), curve.y.Evaluate(t), curve.x.Evaluate(t), curve.w.Evaluate(t));
            return true;
        }

        public void AddPosition(int boneId, string property, AnimationCurve curve)
        {
            if (!m_Positions.TryGetValue(boneId, out var value))
            {
                value = new AnimationCurvePosition();
                m_Positions.Add(boneId, value);
            }
            value.SetValue(property, curve);
        }

        public void AddRotation(int boneId, string property, AnimationCurve curve)
        {
            if (!m_Rotations.TryGetValue(boneId, out var value))
            {
                value = new AnimationCurveRotation();
                m_Rotations.Add(boneId, value);
            }
            value.SetValue(property, curve);
        }
    }

    public class EntityAnimatorConfig : ScriptableIdentifiable, IInitiated
    {
        public const string ROOT_NAME = "_root_";
        
        [HideInInspector]
        [SerializeField] private AnimatorItem [] m_Items;
#if UNITY_EDITOR         
        [SerializeField] private AnimatorController m_AnimatorController;

        private Dictionary<ObjectID, EntityClip> m_Clips = new Dictionary<ObjectID, EntityClip>();

        public EntityClip GetClip(ObjectID id) => GetClip(id, false); 

        EntityClip GetClip(ObjectID id, bool need)
        {
            if (m_Clips.TryGetValue(id, out var clip) || !need) return clip;
            
            clip = new EntityClip();
            m_Clips.Add(id, clip);
            return clip;
        }

        void IInitiated.Init()
        {
            BuildHash();
        }
        
        void BuildHash()
        {
            foreach (var iter in m_Items)
            {
                var clip = GetClip(iter.ID, need: true);
                clip.AddPosition(iter.HashCode, iter.PropertyName, iter.Curve);
                clip.AddRotation(iter.HashCode, iter.PropertyName, iter.Curve);
            }
        }
        
        public override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
            m_Items = new AnimatorItem[]{};
            if (!m_AnimatorController) return;
            
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
        private struct AnimatorItem
        {
            public ObjectID ID;
            public AnimationCurve Curve;
            public string PropertyName;
            public int HashCode;
        }
    }
}