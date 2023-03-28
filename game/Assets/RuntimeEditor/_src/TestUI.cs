using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TestUI : MonoBehaviour
{
    [SerializeField]
    UIDocument m_ChoiceEnvironment;
    [SerializeField]
    UIDocument m_Dialog;

    private void Start()
    {
        m_ChoiceEnvironment.gameObject.SetActive(true);
        ///UnityEngine.UIElements.even

        m_ChoiceEnvironment.rootVisualElement.RegisterCallback<ClickEvent>(evt => 
        {
            Debug.Log("Miss click");
        });
    }
}
