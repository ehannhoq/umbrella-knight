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
    public bool isGrounded;
    public bool onJumpCooldown;

    void Start()
    {
        _player = GameObject.FindWithTag("Player");
        _cam = GameObject.FindWithTag("MainCamera");
        _animator = _player.GetComponent<Animator>();
        _movementSpeedMultipliers = new Dictionary<string, float>();
        _linearDampening = _rb.linearDamping;

        _rb.freezeRotation = true;
        // Cursor.visible = false;
    }


    void FixedUpdate()
    {
        // Debug.DrawRay(player.transform.position, -player.transform.up * 0.1f, Color.red, 1f, false);

        isGrounded = Math.Abs(_rb.linearVelocity.y) < 0.001f || Physics.Raycast(_player.transform.position, -_player.transform.up, 0.1f);

        if (isGrounded)
            _rb.linearDamping = _linearDampening;
        else
        {
            _rb.linearDamping = 0;
            _rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
        }

        HandleMovement();
    }   

    void HandleMovement()
    {
        Vector3 movementVector = ((new Vector3(_cam.transform.forward.x, 0f, _cam.transform.forward.z) * _moveInput.y) + (_cam.transform.right * _moveInput.x)) * _currentSpeed;
        _rb.linearVelocity = new Vector3(movementVector.x, _rb.linearVelocity.y, movementVector.z);

        if (_moveInput != Vector2.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(movementVector);
            _player.transform.rotation = Quaternion.Slerp(_player.transform.rotation, targetRot, Time.fixedDeltaTime * 10f);
        }

        if (isGrounded)
        {
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


        _animator.SetBool("Walking", _moveInput != Vector2.zero);
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
