using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Buildings
{
    public class Config : ScriptableObject
    {
        [SerializeField]
        private InputActionReference m_MoveAction;

        [SerializeField]
        private InputActionReference m_PlaceAction;

        public InputAction MoveAction => m_MoveAction.action;
        public InputAction PlaceAction => m_PlaceAction.action;
    }
}