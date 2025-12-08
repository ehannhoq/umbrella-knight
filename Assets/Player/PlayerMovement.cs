using System;
using System.Collections;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private enum WallRideDirection : int
    {
        None,
        Left,
        Right
    }

    [Header("Horizontal Movement")]
    [SerializeField] private float _minStepHeight;
    [SerializeField] private float _maxStepHeight;
    [SerializeField] private float _minimumWallRideSpeed;
    [SerializeField] private float _maximumDistanceFromWallForWallRide;
    [SerializeField] private float _wallRideMovementSpeedIncrease;

    [Header("Vertical Movement")]
    [SerializeField] private float _groundedMatchTime;
    [SerializeField] private float _jumpHeight;
    [SerializeField] private float _jumpCooldown;

    [Header("Gravity")]
    [SerializeField] private float _gravity;
    [SerializeField] private float _glidingGravity;
    [SerializeField] private float _wallRideGravity;
    [SerializeField] private float _stickToGroundForce;
    [SerializeField] private float ascendingFallingThreshold;

    private PlayerStats _playerStats;
    private Rigidbody _rb;
    private Animator _anim;
    private GameObject _camera;
    private UmbrellaManager _umbrellaManager;

    private Vector2 _moveInput;
    private RaycastHit _groundHit;
    private int _wallRiding;
    private float _playerHeight = 1.8f;
    private bool canJump;
    private bool trueGrounded;
    private Coroutine matchGroundedCoroutine;

    [Header("Public Variables")]
    public bool canMove;
    public bool grounded;
    public bool gliding;
    public bool ascending;
    public bool falling;


    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");

        _playerStats = GetComponent<PlayerStats>();
        _rb = player.GetComponent<Rigidbody>();
        _anim = player.GetComponent<Animator>();
        _camera = GameObject.FindWithTag("MainCamera");
        _umbrellaManager = GetComponent<UmbrellaManager>(); 

        _rb.freezeRotation = true;
        canMove = true;
        canJump = true;
    }

    void FixedUpdate()
    {
        CheckGrounded();
        StickToGround();
        HandleMovement();
        AdjustVelocity();
        SetAnimation();

        if (grounded != trueGrounded && matchGroundedCoroutine == null)
        {
            StartCoroutine(MatchGrounded());
        }

        _rb.linearDamping = trueGrounded ? 10 : 1;
        ascending = _rb.linearVelocity.y > ascendingFallingThreshold;
        falling = _rb.linearVelocity.y < -ascendingFallingThreshold;

        if (_wallRiding != 0)
            _rb.AddForce(Physics.gravity * _wallRideGravity, ForceMode.Acceleration);
        else if (gliding)
            _rb.AddForce(Physics.gravity * _glidingGravity, ForceMode.Acceleration);
        else
            _rb.AddForce(Physics.gravity * _gravity, ForceMode.Acceleration);

        if (_umbrellaManager.blocking)
            RotatePlayer(Vector3.ProjectOnPlane(_camera.transform.forward, Vector3.up).normalized, true);
    }

    void CheckGrounded()
    {
        trueGrounded = false;

        if (Physics.SphereCast(
            _rb.position + Vector3.up * 0.35f,
            0.15f,
            Vector3.down,
            out _groundHit,
            0.3f,
            Util.nonColliderMasks,
            QueryTriggerInteraction.Ignore
            ))
        {
            trueGrounded = Vector3.Dot(_groundHit.normal, Vector3.up) > 0.6f;
        }
    }

    void StickToGround()
    {
        if (!trueGrounded) return;

        float yDiff = _groundHit.point.y - _rb.transform.position.y;
        if (yDiff > 0f && yDiff <= _minStepHeight)
        {
            _rb.MovePosition(new Vector3(_rb.transform.position.x, _groundHit.point.y, _rb.transform.position.z));
        }
        else if (yDiff < 0f)
            _rb.AddForce(Vector3.down * _stickToGroundForce, ForceMode.Force);
    }

    void HandleMovement()
    {
        if (!canMove) return;

        if (_moveInput != Vector2.zero)
        {
            Vector3 cameraForward = Vector3.ProjectOnPlane(_camera.transform.forward, Vector3.up);
            cameraForward.y = 0;
            cameraForward = cameraForward.normalized;

            Vector3 movement = (cameraForward * _moveInput.y + _camera.transform.right * _moveInput.x) * _playerStats.movementSpeed;

            if (_wallRiding != 0)
                movement *= _wallRideMovementSpeedIncrease;

            Step(movement);
            movement = AdjustForWall(movement);
            RotatePlayer(movement);

            movement -= new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            _rb.AddForce(movement, trueGrounded ? ForceMode.VelocityChange : ForceMode.Acceleration);
        }

        CheckForWallRide();
    }


    void CheckForWallRide()
    {
        if (grounded || _moveInput == Vector2.zero || _rb.linearVelocity.sqrMagnitude < _minimumWallRideSpeed * _minimumWallRideSpeed)
        {
            _wallRiding = (int)WallRideDirection.None;
            return;
        }

        Vector3 right = _camera.transform.right;
        Vector3 left = -right;


        if (CheckWall(right, out RaycastHit rightHit))
        {
            float vertical = Vector3.Dot(rightHit.normal, Vector3.up);
            if (vertical < 0.1f)
                _wallRiding = (int)WallRideDirection.Right;
        }

        else if (CheckWall(left, out RaycastHit leftHit))
        {
            float vertical = -Vector3.Dot(leftHit.normal, Vector3.up);
            if (vertical < 0.1f)
                _wallRiding = (int)WallRideDirection.Left;
        }
        else
        {
            _wallRiding = (int)WallRideDirection.None;
        }
    }

    bool CheckWall(Vector3 direction, out RaycastHit hit)
    {
        float radius = 0.175f;
        Vector3 top = _rb.position + Vector3.up * (_playerHeight - 0.3f);
        Vector3 bottom = _rb.position + Vector3.up * 0.3f;

        if (Physics.CapsuleCast(bottom, top, radius, direction, out hit, _maximumDistanceFromWallForWallRide, Util.nonColliderMasks))
            return true;

        hit = new RaycastHit();
        return false;
    }


    void Step(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f || !grounded)
        {
            if (direction.sqrMagnitude < 0.001f) return;
        }

        Vector3 lower = _rb.transform.position + Vector3.down * _minStepHeight;
        Vector3 upper = _rb.transform.position + Vector3.up * _maxStepHeight;

        // Color lowerColor = Color.red;
        // Color upperColor = Color.red;

        if (Physics.Raycast(lower, direction.normalized, out RaycastHit hit, 0.25f, ~LayerMask.GetMask("Player"), QueryTriggerInteraction.Collide))
        {
            if (Vector3.Dot(hit.normal, Vector3.up) > 0.25f) return;

            // lowerColor = Color.green;

            if (!Physics.Raycast(upper, direction.normalized, 0.25f, ~LayerMask.GetMask("Player")))
            {
                float stepHeight = hit.collider.bounds.max.y - _rb.transform.position.y;
                if (stepHeight < _minStepHeight) return;
                // upperColor = Color.green;

                stepHeight = Mathf.Clamp(stepHeight, 0f, _maxStepHeight);
                _rb.transform.position += new Vector3(0f, stepHeight, 0f);
            }
        }

        // Debug.DrawLine(lower, lower + direction.normalized * 0.25f, lowerColor);
        // Debug.DrawLine(upper, upper + direction.normalized * 0.25f, upperColor);

    }

    public void AdjustVelocity()
    {
        Vector3 v = _rb.linearVelocity;
        v = AdjustForSlope(v);
        v = AdjustForWall(v);
        _rb.linearVelocity = v;
    }

    Vector3 AdjustForSlope(Vector3 velocity)
    {
        if (!trueGrounded) return velocity;

        return Vector3.ProjectOnPlane(velocity, _groundHit.normal);
    }
    Vector3 AdjustForWall(Vector3 velocity)
    {
        if (velocity.sqrMagnitude < 0.01f) return velocity;

        Vector3 p1 = _rb.position + Vector3.up * (0.3f);
        Vector3 p2 = _rb.position + Vector3.up * (_playerHeight - 0.3f);

        float castDistance = Mathf.Max(0.25f, velocity.magnitude * Time.fixedDeltaTime);
        if (Physics.CapsuleCast(
            p1,
            p2,
            0.175f,
            velocity.normalized,
            out RaycastHit wallHit,
            castDistance,
            Util.nonColliderMasks
        ))
        {
            Vector3 normal = wallHit.normal;
            normal.y = 0;
            Vector3 projected = Vector3.ProjectOnPlane(velocity, normal);
            return projected;
        }

        return velocity;
    }

    void SetAnimation()
    {
        _anim.SetBool("Grounded", trueGrounded);
        _anim.SetBool("Moving", _moveInput != Vector2.zero);
        _anim.SetBool("Ascending", ascending);
        _anim.SetBool("Falling", falling);
        _anim.SetInteger("WallRiding", _wallRiding);
    }

    public void RotatePlayer(Vector3 lookVector, bool instant = false)
    {
        Quaternion rotation = Quaternion.LookRotation(lookVector);
        _rb.transform.rotation = instant ? rotation : Quaternion.Slerp(_rb.transform.rotation, rotation, 10f * Time.deltaTime);
    }


    public void OnMove(InputValue input)
    {
        _moveInput = input.Get<Vector2>();
    }

    public void OnJump()
    {
        if (!canJump) return;
        canJump = true;

        if (grounded)
            _rb.AddForce(Vector3.up * _jumpHeight, ForceMode.Impulse);
        else if (_wallRiding != 0)
        {
            Vector3 dir;

            if (_wallRiding == (int)WallRideDirection.Right)
                dir = _camera.transform.right;
            else
                dir = -_camera.transform.right;

            CheckWall(dir, out RaycastHit hit);

            Quaternion rot = Quaternion.AngleAxis(-45f, Vector3.up);
            Vector3 sideVector = (rot * hit.normal).normalized;

            Vector3 forwardVector = Vector3.ProjectOnPlane(_camera.transform.forward, hit.normal).normalized;
            forwardVector.y = 0;
            Vector3 jumpDirection = (sideVector + forwardVector * 1.25f).normalized * _jumpHeight;
            _rb.AddForce(jumpDirection, ForceMode.VelocityChange);
        }

        StartCoroutine(JumpCooldown());
    }

    IEnumerator JumpCooldown()
    {
        yield return new WaitForSeconds(_jumpCooldown);
        canJump = true;
    }

    IEnumerator MatchGrounded()
    {
        yield return new WaitForSeconds(_groundedMatchTime);
        grounded = trueGrounded;
        matchGroundedCoroutine = null;
    }
}