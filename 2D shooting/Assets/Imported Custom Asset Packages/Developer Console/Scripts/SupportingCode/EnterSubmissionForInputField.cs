using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class EnterSubmissionForInputField : MonoBehaviour
{
    public GameObject requiredSelectedObject;

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame && EventSystem.current.currentSelectedGameObject == requiredSelectedObject)
        {
            DeveloperConsole.instance.ParseCommand();
        }
    }
}
