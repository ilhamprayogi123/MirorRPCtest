using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enable and Disable cursor
public class HidelockCursor : MonoBehaviour
{
    public bool lockCursor;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            lockCursor = !lockCursor;
        }

        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockCursor;
    }
}
