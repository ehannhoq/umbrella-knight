using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    Camera _cam;
    GameObject _player;
    Rigidbody _rb;
    public Vector2 _lookVec;
    float _baseFOV;
    [SerializeField] float _maxFOV;
    [SerializeField] float _velocityThreshold;
    // Vector3 lockOnPos;

    public float xSensitivity;
    public float ySensitivity;

    public static bool cameraLock = false;
    public float cameraDistance;
    public float yaw;
    public float pitch;
    public Vector3 offset;

    void Start()
    {
        _cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        _player = GameObject.FindWithTag("Player");
        _rb = _player.GetComponent<Rigidbody>();

        _baseFOV = _cam.fieldOfView;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void FixedUpdate()
    {
        float velocity = _rb.linearVelocity.magnitude;

        if (velocity < _velocityThreshold) 
        {
            _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, _baseFOV, 0.25f);
            return;
        }

        float fovDiff = _maxFOV - _baseFOV;

        float fov = _baseFOV + (fovDiff * ((velocity - _velocityThreshold) / 50f));

        _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, fov, 0.25f);
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
            yaw += _lookVec.x * xSensitivity * Time.deltaTime;
            pitch -= _lookVec.y * xSensitivity * Time.deltaTime;

            pitch = Math.Clamp(pitch, -40f, 80f);
            yaw = yaw % 360;

            _cam.transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }


    private void UpdatePosition()
    {
        int layerMask = ~(LayerMask.GetMask("Player") | LayerMask.GetMask("Ignore Collision"));
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
