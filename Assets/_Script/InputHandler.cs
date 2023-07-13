using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputHandler : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _InputField;

    [SerializeField]
    private Button inputButton;

    public string InputText { get { return _InputField.text; } }

    public void ToggleInputField()
    {
        _InputField.gameObject.SetActive(!_InputField.gameObject.activeSelf);
        inputButton.gameObject.SetActive(!inputButton.gameObject.activeSelf);
    }
}
