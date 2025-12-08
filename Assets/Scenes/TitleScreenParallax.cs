using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TitleScreenParallax : MonoBehaviour
{
    private GameObject cameraObj;
    private GameObject playerObj;

    [SerializeField] private float parallaxAmount;

    private Transform baseCameraTransform;
    private Vector3 baseCameraForwardEuler;

    void Start()
    {
        cameraObj = GameObject.FindWithTag("MainCamera");
        playerObj = GameObject.FindWithTag("Player");

        baseCameraTransform = cameraObj.transform;
        baseCameraForwardEuler = cameraObj.transform.rotation.eulerAngles;
    }

    void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        mousePos.x /= Screen.width;
        mousePos.y /= Screen.height;

        Quaternion targetRot = Quaternion.Euler(
            baseCameraForwardEuler.x + mousePos.y * parallaxAmount,
            baseCameraForwardEuler.y + mousePos.x * parallaxAmount,
            baseCameraForwardEuler.z
        );

        cameraObj.transform.rotation = Quaternion.Slerp(cameraObj.transform.rotation, targetRot, 5f * Time.deltaTime);
        
        Vector3 cameraFlippedRot = cameraObj.transform.rotation.eulerAngles + new Vector3(360f, 360f, 360f);
        playerObj.transform.rotation = Quaternion.Euler(cameraFlippedRot);
    }
}
