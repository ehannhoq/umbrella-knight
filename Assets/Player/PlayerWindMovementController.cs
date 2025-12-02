using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWindMovementController : MonoBehaviour
{
    GameObject _cam;
    Rigidbody _rb;
    PlayerMovement _movement;
    UmbrellaManager _umbrellaManager;
    InputAction _jump;
    Coroutine _windBoostRoutine;

    public float boost;
    public float jumpBoost;
    public bool inWindDash;
    void OnEnable()
    {
        var playerInput = GetComponent<PlayerInput>();
        _jump = playerInput.actions["Jump"];
        _jump.Enable();
    }

    void Start()
    {
        _cam = GameObject.FindWithTag("MainCamera");
        _rb = GameObject.FindWithTag("Player").GetComponent<Rigidbody>();
        _movement = GetComponent<PlayerMovement>();
        _umbrellaManager = GetComponent<UmbrellaManager>();
        inWindDash = false;
    }

    public void OnWindBoost()
    {
        if (_umbrellaManager.umbrellaState == UmbrellaState.Closed || !_movement.grounded) return;

        _windBoostRoutine = StartCoroutine(WindBoost());
    }


    IEnumerator WindBoost()
    {
        Vector3 projectedVector = Vector3.ProjectOnPlane(_cam.transform.forward, Vector3.up).normalized;
        Vector3 dir = projectedVector * boost;

        _movement.canMove = false;
        inWindDash = true;

        _rb.linearVelocity = dir;

        int duration = 15;
        int timer = 0;
        while (timer < duration)
        {
            timer++;

            yield return new WaitForFixedUpdate();
        }

        _movement.canMove = true;
        inWindDash = false;
    }

    public void OnJump()
    {
        if (!inWindDash) return;

        StopCoroutine(_windBoostRoutine);
        inWindDash = false;
        _movement.canMove = true;
        _rb.linearVelocity *= 0.33f;

        Vector3 dir = _rb.transform.up * jumpBoost;

        _rb.AddForce(dir, ForceMode.VelocityChange);

        Debug.Log("Wind Jump");
    }
}
