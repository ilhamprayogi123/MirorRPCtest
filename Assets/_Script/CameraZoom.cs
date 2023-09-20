using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    CinemachineComponentBase componentBase;
    float cameraDistance;
    //[SerializeField] float sensitivity = 10f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            cameraDistance = virtualCamera.m_Lens.FieldOfView += 20;

            if (cameraDistance >= 80)
            {
                virtualCamera.m_Lens.FieldOfView = 80f;
            }
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            cameraDistance = virtualCamera.m_Lens.FieldOfView -= 20;

            if (cameraDistance <= 40)
            {
                virtualCamera.m_Lens.FieldOfView = 40f;
            }
        }
    }
}
