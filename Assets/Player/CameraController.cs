using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    GameObject _cam;
    GameObject _player;
    Vector2 _lookVec;
    // Vector3 lockOnPos;

    public static bool cameraLock = false;
    public float cameraDistance;
    public float yaw;
    public float pitch;
    public Vector3 offset;

    void Start()
    {
        _cam = GameObject.FindWithTag("MainCamera");
        _player = GameObject.FindWithTag("Player");

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        UpdateRotation();
        UpdatePosition();
    }

    private void UpdateRotation()
    {
        if (_lookVec != Vector2.zero)
        {
            yaw += _lookVec.x;
            pitch -= _lookVec.y;

            pitch = Math.Clamp(pitch, -40f, 80f);
            yaw = yaw % 360;

            _cam.transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }

    private void UpdatePosition()
    {
        int layerMask = ~LayerMask.GetMask("Player");
        Vector3 lookAtPos = _player.transform.position + offset;

        if (Physics.Raycast(lookAtPos, -_cam.transform.forward, out RaycastHit info, cameraDistance, layerMask))
        {                
            float collisionOffset = 0.2f;

            float newDistance = info.distance - collisionOffset;
            newDistance = Math.Clamp(newDistance, 0.01f, cameraDistance);

            _cam.transform.position = lookAtPos - _cam.transform.forward * newDistance;
        }
        else
        {
            _cam.transform.position = lookAtPos - _cam.transform.forward * cameraDistance;
        }
    }



    public void OnLook(InputValue action)
    {
        _lookVec = action.Get<Vector2>();
    }
}
