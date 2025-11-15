using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWindMovementController : MonoBehaviour
{
    GameObject _cam;
    Rigidbody _rb;
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
        _umbrellaManager = GetComponent<UmbrellaManager>();
        inWindDash = false;

        PlayerMovement.playerJumped += () => { if (inWindDash) StartCoroutine(JumpedDuringWindBoost()); };
    }

    public void OnWindBoost()
    {
        if (_umbrellaManager.umbrellaState == UmbrellaState.Closed) return;

        _windBoostRoutine = StartCoroutine(WindBoostCoroutine());
    }

    IEnumerator WindBoostCoroutine()
    {
        Vector3 projectedVector = Vector3.ProjectOnPlane(_cam.transform.forward, Vector3.up).normalized;
        Vector3 dir = projectedVector * boost * 1000;

        int duration = 10;
        int timer = 0;

        while (timer < duration)
        {
            _rb.AddForce(dir, ForceMode.Acceleration);
            timer++;
            inWindDash = true;

            yield return new WaitForFixedUpdate();
        }
        inWindDash = false;
    }

    IEnumerator JumpedDuringWindBoost()
    {
        StopCoroutine(_windBoostRoutine);
        inWindDash = false;
        _rb.linearVelocity = Vector3.zero;

        Vector3 dir = _rb.transform.up * jumpBoost * 50;

        int duration = 5;
        int timer = 0;


        while (timer < duration)
        {
            _rb.AddForce(dir, ForceMode.Acceleration);
            timer++;
            yield return new WaitForFixedUpdate();
        }
    }
}
