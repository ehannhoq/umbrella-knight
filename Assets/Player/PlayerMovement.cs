using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private enum WallRideDirection : int
    {
        None,
        Left,
        Right
    }
    public static Action playerJumped;
    [SerializeField] float _playerHeight;
    [SerializeField] private float _movementSpeed;
    [SerializeField] private Dictionary<string, float> _movementSpeedMultipliers;
    [SerializeField] private float _maxStepHeight = 0.5f;
    [SerializeField] private float _minStepHeight = 0.5f;
    [SerializeField] private float _stickToGroundForce = 10f;

    [SerializeField] float _jumpHeight;

    [Tooltip("In Seconds")]
    [SerializeField] float _jumpCooldown;
    Rigidbody _rb;
    float _currentSpeed;
    GameObject _player;
    GameObject _cam;
    Vector2 _moveInput;
    Animator _animator;
    float _linearDampening;
    [SerializeField] int _wallRiding;
    [SerializeField] float _wallRideGravity;

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
        _rb = _player.GetComponent<Rigidbody>();
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

        isGrounded = Physics.CheckSphere(
            _player.transform.position,
            0.25f,
            ~LayerMask.GetMask("Player")
        );

        if (isGrounded)
        {
            _rb.linearDamping = _linearDampening;

            RaycastHit hit;
            if (Physics.Raycast(_player.transform.position + Vector3.up * 0.1f, Vector3.down, out hit, _minStepHeight + 0.2f))
            {
                float stepDifference = hit.point.y - _player.transform.position.y;
                if (stepDifference > 0f && stepDifference <= _minStepHeight)
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
            _rb.linearDamping = 1;

            Vector3 gravity = Physics.gravity;

            if (isGliding) gravity *= glidingMultiplier;
            else if (_wallRiding != 0) gravity *= _wallRideGravity;
            else gravity *= gravityMultiplier;

            _rb.AddForce(gravity, ForceMode.Acceleration);
        }

        isAscending = _rb.linearVelocity.y > ascendingFallingThreshold;
        isFalling = _rb.linearVelocity.y < -ascendingFallingThreshold;

        _animator.SetBool("Ascending", isAscending);
        _animator.SetBool("Grounded", isGrounded);
        _animator.SetBool("Falling", isFalling);
        _animator.SetInteger("WallRide", _wallRiding);


        if (canMove)
        {
            HandleMovement();
        }
    }


    void HandleMovement()
    {
        if (_moveInput != Vector2.zero)
        {
            Vector3 projectedForward = Vector3.ProjectOnPlane(_cam.transform.forward, Vector3.up).normalized;
            Vector3 movementVector = ((new Vector3(projectedForward.x, 0f, projectedForward.z) * _moveInput.y) + (_cam.transform.right * _moveInput.x)) * _currentSpeed;
            if (!isGrounded) movementVector *= 0.6f;

            Step(movementVector);
            movementVector = AdjustForSlope(movementVector);
            movementVector = AdjustForWall(movementVector);

            movementVector -= new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);

            _rb.AddForce(movementVector, isGrounded ? ForceMode.Impulse : ForceMode.Acceleration);

            SetPlayerRotationToCameraRotation(movementVector, slerp: true);
        }

        CheckForWallRide();

        _animator.SetBool("Walking", _moveInput != Vector2.zero);
    }


    void Step(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f || !isGrounded) return;
        Vector3 lower = _player.transform.position;
        Vector3 upper = _player.transform.position + Vector3.up * _maxStepHeight;


        if (Physics.Raycast(lower, direction.normalized, out RaycastHit hit, 0.75f))
        {

            if (Vector3.Dot(hit.normal, Vector3.up) > 0.1f) return;


            if (!Physics.Raycast(upper, direction.normalized, 0.75f))
            {
                float stepHeight = hit.collider.bounds.max.y - _player.transform.position.y;
                if (stepHeight < 0.1f) return;

                stepHeight = Mathf.Clamp(stepHeight, 0f, _maxStepHeight);
                _player.transform.position += new Vector3(0f, stepHeight, 0f);
            }
        }
    }


    Vector3 AdjustForSlope(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f || !isGrounded) return direction;

        Physics.Raycast(_player.transform.position, Vector3.down, out RaycastHit hit, 0.75f);

        return Vector3.ProjectOnPlane(direction, hit.normal);
    }


    Vector3 AdjustForWall(Vector3 direction)
    {

        Vector3 p1 = _player.transform.position + Vector3.up * (_playerHeight / 2f - 0.2f);
        Vector3 p2 = _player.transform.position + Vector3.up * (_playerHeight / 2f + 0.2f);

        if (Physics.CapsuleCast(
            p1, p2,
            0.3f,
            direction.normalized,
            out RaycastHit hit,
            0.175f
        ))
        {
            Vector3 horizontalNormal = hit.normal;
            horizontalNormal.y = 0f;
            horizontalNormal = horizontalNormal.normalized;

            Vector3 newDirection = Vector3.ProjectOnPlane(direction, horizontalNormal);

            if (_wallRiding != 0)
                newDirection = newDirection.normalized * direction.magnitude;

            return newDirection;
        }

        return direction;
    }

    void CheckForWallRide()
    {
        if (isGrounded || _rb.linearVelocity.sqrMagnitude <= _movementSpeed * _movementSpeed)
        {
            _wallRiding = (int)WallRideDirection.None;
            return;
        }

        Vector3 p1 = _player.transform.position + Vector3.up * (_playerHeight / 2f - 0.2f);

        Vector3 right = _cam.transform.right;
        Vector3 left = -right;

        _wallRiding = (int)WallRideDirection.None;

        if (GetWallHit(p1, right, 0.4f, out RaycastHit rightHit))
        {
            float vertical = Vector3.Dot(rightHit.normal, Vector3.up);
            if (vertical < 0.1f)
                _wallRiding = (int)WallRideDirection.Right;
        }

        if (GetWallHit(p1, left, 0.4f, out RaycastHit leftHit))
        {
            float vertical = -Vector3.Dot(leftHit.normal, Vector3.up);
            if (vertical < 0.1f)
                _wallRiding = (int)WallRideDirection.Left;
        }
    }

    bool GetWallHit(Vector3 p1, Vector3 direction, float maxDistance, out RaycastHit hit)
    {
        if (Physics.Raycast(p1, direction, out RaycastHit h, maxDistance))
        {
            hit = h;
            return true;
        }
        hit = new RaycastHit();
        return false;
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

    public void OnMove(InputValue action)
    {
        _moveInput = action.Get<Vector2>();
    }


    public void OnJump()
    {
        if (!onJumpCooldown)
        {
            onJumpCooldown = true;
            if (isGrounded)
            {
                _rb.AddForce(_rb.transform.up * _jumpHeight, ForceMode.Impulse);
            }
            else if (_wallRiding != 0)
            {
                Vector3 p1 = _player.transform.position + Vector3.up * (_playerHeight / 2f - 0.2f);
                Vector3 dir;

                if (_wallRiding == (int)WallRideDirection.Right)
                    dir = _cam.transform.right;
                else
                    dir = -_cam.transform.right;

                GetWallHit(p1, dir, 0.4f, out RaycastHit hit);

                Vector3 axis = Quaternion.AngleAxis(90f, Vector3.up) * hit.normal;
                Quaternion rot = Quaternion.AngleAxis(-45f, axis);

                Vector3 jumpVector = (rot * hit.normal).normalized * _jumpHeight;

                _rb.AddForce(jumpVector, ForceMode.Impulse);

                Debug.DrawLine(_player.transform.position, _player.transform.position + jumpVector, Color.yellow);

            }
            playerJumped.Invoke();
            StartCoroutine(ResetJump());
        }
    }

    IEnumerator WallJump(Vector3 jumpVector)
    {
        int duration = 4;
        while (duration > 0)
        {
            yield return new WaitForFixedUpdate();
            duration--;
            _rb.AddForce(jumpVector * 10f, ForceMode.Acceleration);
            jumpVector.y *= 0.5f;
        }
    }

    IEnumerator ResetJump()
    {
        yield return new WaitForSeconds(_jumpCooldown);
        onJumpCooldown = false;
    }

    public void AddSpeedMultiplier(string source, float multiplier)
    {
        _movementSpeedMultipliers.Add(source, multiplier);
    }


    public void RemoveSpeedMultiplier(string source)
    {
        if (_movementSpeedMultipliers.ContainsKey(source))
            _movementSpeedMultipliers.Remove(source);
    }
}
