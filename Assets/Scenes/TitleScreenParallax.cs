using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TitleScreenParallax : MonoBehaviour
{
    private GameObject cameraObj;
    private GameObject playerObj;

    [SerializeField] private float parallaxAmount;

    public Vector2 mousePos;
    private Vector3 baseCameraForwardEuler;

    void Start()
    {
        cameraObj = GameObject.FindWithTag("MainCamera");
        playerObj = GameObject.FindWithTag("Player");

        baseCameraForwardEuler = cameraObj.transform.rotation.eulerAngles;
    }

    void Update()
    {
        mousePos = Mouse.current.position.ReadValue();
        mousePos.x = (mousePos.x / (Screen.width / 2)) - 1;
        mousePos.y = (mousePos.y / (Screen.height / 2)) - 1;

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
