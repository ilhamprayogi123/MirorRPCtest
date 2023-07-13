using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _inputPrefab = null;

    [SerializeField]
    private Transform _inputPrefabParent = null;

    private InputHandler _inputHandler = null;

    public void CreateInputInstance()
    {
        // Create our instance from the prefab.
        var instance = Instantiate(_inputPrefab, _inputPrefabParent, false);
        //Get the script attached to the gameObject.
        _inputHandler = instance.GetComponent<InputHandler>();
    }

    public void GetText()
    {
        Debug.Log($"Input Field Value: {_inputHandler.InputText}");
    }
}
