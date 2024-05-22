using System;
using System.Collections.Generic;
using System.Linq;

using Game.Core;
using Game.Model.Logics;

using Unity.Collections;

using UnityEngine;

namespace Game.Views
{
    [RequireComponent(typeof(Animator))]
    public class ViewAnimation : MonoBehaviour, IViewLogicComponent
    {
        public Animator Animator { get; private set; }

        [SerializeField] 
        private List<StateItem> m_StateItems;

        private Map<LogicActionHandle, AnimationValue> m_States = new(5, Allocator.Persistent, true);

        public void ChangeAction(LogicActionHandle action)
        {
            if (!TryGetState(action, out var values)) return;

            foreach ((string animatorParam, bool animatorValue) in values)
            {
                if (animatorValue != Animator.GetBool(animatorParam))
                    Animator.SetBool(animatorParam, animatorValue);
            }
        }

        private void Awake()
        {
            Animator = GetComponent<Animator>();
            foreach (var iter in m_StateItems)
            {
                m_States.Add(
                    LogicActionHandle.FromType(Type.GetType(iter.State)),
                    new AnimationValue 
                    {
                        Name = iter.AnimationParam,
                        Value = iter.AnimationValue,
                    });
            }
        }

        private bool TryGetState(LogicActionHandle action, out IEnumerable<(string animatorParam, bool animatorValue)> param)
        {
            param = null;
            if (!m_States.TryGetValues(action, out var values)) return false;

            param = values.Select(iter => (iter.Name.ToString(), iter.Value));
            return true;
        } 
        
        private struct AnimationValue
        {
            public FixedString64Bytes Name;
            public bool Value;
        }

        [Serializable]
        private struct StateItem
        {
            [SerializeField, SelectType(typeof(Logic.IAction))]
            public string State;
            public string AnimationParam;
            public bool AnimationValue;
        }
    }
}
