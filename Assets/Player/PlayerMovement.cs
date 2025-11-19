using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public static Action playerJumped;
    [SerializeField] Rigidbody _rb;
    [SerializeField] private float _movementSpeed;
    [SerializeField] private Dictionary<string, float> _movementSpeedMultipliers;
    [SerializeField] private float _stepOffset = 0.5f;
    [SerializeField] private float _stickToGroundForce = 10f;
    [SerializeField] float _jumpHeight;
    [SerializeField] float _playerHeight;

    [Tooltip("In Seconds")]
    [SerializeField] float _jumpCooldown;
    float _currentSpeed;
    GameObject _player;
    GameObject _cam;
    Vector2 _moveInput;
    Animator _animator;
    float _linearDampening;

    public float gravityMultiplier;
    public float glidingMultiplier;
    public bool isGrounded;
    public bool onJumpCooldown;
    public bool canMove;
    public bool isAscending;
    public bool isFalling;
    public bool isGliding;
    public float ascendingFallingThreshold;

    void Start()
    {
        _player = GameObject.FindWithTag("Player");
        _cam = GameObject.FindWithTag("MainCamera");
        _animator = _player.GetComponent<Animator>();
        _movementSpeedMultipliers = new Dictionary<string, float>();
        _linearDampening = _rb.linearDamping;

        canMove = true;

        _rb.freezeRotation = true;
        // Cursor.visible = false;
    }


    void FixedUpdate()
    {
        // Debug.DrawRay(player.transform.position, -player.transform.up * 0.1f, Color.red, 1f, false);

        isGrounded = Math.Abs(_rb.linearVelocity.y) < 0.001f || Physics.Raycast(_player.transform.position, -_player.transform.up, 0.1f);

        if (isGrounded)
        {
            _rb.linearDamping = _linearDampening;

            RaycastHit hit;
            if (Physics.Raycast(_player.transform.position + Vector3.up * 0.1f, Vector3.down, out hit, _stepOffset + 0.2f))
            {
                float stepDifference = hit.point.y - _player.transform.position.y;
                if (stepDifference > 0f && stepDifference <= _stepOffset)
                {
                    _rb.MovePosition(new Vector3(_player.transform.position.x, hit.point.y, _player.transform.position.z));
                }
                else if (stepDifference < 0f)
                {
                    _rb.AddForce(Vector3.down * _stickToGroundForce, ForceMode.Force);
                }
            }
        }
        else
        {
            _rb.linearDamping = 0;
            _rb.AddForce(Physics.gravity * (isGliding ? glidingMultiplier : gravityMultiplier), ForceMode.Acceleration);
        }

        isAscending = _rb.linearVelocity.y > ascendingFallingThreshold;
        isFalling = _rb.linearVelocity.y < -ascendingFallingThreshold;

        _animator.SetBool("Ascending", isAscending);
        _animator.SetBool("Falling", isFalling);
        _animator.SetBool("Gliding", isGliding);


        if (canMove)
        {
            HandleMovement();
        }


    }


    void HandleMovement()
    {
        Vector3 projectedForward = Vector3.ProjectOnPlane(_cam.transform.forward, Vector3.up).normalized;
        Vector3 movementVector = ((new Vector3(projectedForward.x, 0f, projectedForward.z) * _moveInput.y) + (_cam.transform.right * _moveInput.x)) * _currentSpeed;
        if (!isGrounded) movementVector *= 0.6f;
        _rb.linearVelocity = new Vector3(movementVector.x, _rb.linearVelocity.y, movementVector.z);

        if (_moveInput != Vector2.zero)
        {
            SetPlayerRotationToCameraRotation(movementVector, slerp: true);
        }

        _animator.SetBool("Walking", _moveInput != Vector2.zero);
    }

    public void SetPlayerRotationToCameraRotation(Vector3 lookVector, bool slerp = false)
    {
        Quaternion targetRot = Quaternion.LookRotation(lookVector);

        if (slerp)
            _player.transform.rotation = Quaternion.Slerp(_player.transform.rotation, targetRot, Time.fixedDeltaTime * 10f);
        else
            _player.transform.rotation = targetRot;
    }

    void Update()
    {
        float cummilativeSpeedMultiplier = 1f;
        foreach (var keyValuePair in _movementSpeedMultipliers)
            cummilativeSpeedMultiplier *= keyValuePair.Value;

        _currentSpeed = _movementSpeed * cummilativeSpeedMultiplier;
    }

    public void AddSpeedMultiplier(string source, float multiplier)
    {
        _movementSpeedMultipliers.Add(source, multiplier);
    }

    public void RemoveSpeedMultiplier(string source)
    {
        _movementSpeedMultipliers.Remove(source);
    }

    public void OnMove(InputValue action)
    {
        _moveInput = action.Get<Vector2>();
    }

    public void OnJump()
    {
        if (!onJumpCooldown && isGrounded)
        {
            onJumpCooldown = true;
            _rb.AddForce(_rb.transform.up * _jumpHeight, ForceMode.VelocityChange);
            playerJumped.Invoke();
            StartCoroutine(ResetJump());
        }
    }

    IEnumerator ResetJump()
    {
        yield return new WaitForSeconds(_jumpCooldown);
        onJumpCooldown = false;
    }
}
