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
        
        [SerializeField]
        private InputActionReference m_CancelAction;
        [SerializeField]
        private InputActionReference m_RorateXAction;
        [SerializeField]
        private InputActionReference m_RorateYAction;

        public InputAction MoveAction => m_MoveAction;
        public InputAction PlaceAction => m_PlaceAction;
        public InputAction CancelAction => m_CancelAction;
        public InputAction RorateXAction => m_RorateXAction;
        public InputAction RorateYAction => m_RorateYAction;

    }
}