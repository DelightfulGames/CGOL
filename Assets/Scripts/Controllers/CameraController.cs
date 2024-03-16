using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private Camera camera;
    public FixedJoystick fixedJoystick;
    private bool isLooking;

    // Start is called before the first frame update
    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    public void OnZoom(InputAction.CallbackContext context)
    {
        if (camera == null)
            return;

        if (context.ReadValue<float>() > 0)
            camera.fieldOfView += 10;
        else if (context.ReadValue<float>() < 0)
            camera.fieldOfView -= 10;
    }

    private void LateUpdate()
    {
        camera.transform.rotation = Quaternion.Euler(-fixedJoystick.Vertical * 10, fixedJoystick.Horizontal * 10, 0);
    }
}