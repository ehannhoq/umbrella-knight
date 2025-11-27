using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum UmbrellaState
{
    Open,
    Closed
}
public class UmbrellaManager : MonoBehaviour
{
    [SerializeField] GameObject _openUmbrella;
    [SerializeField] GameObject _closedUmbrella;
    [SerializeField] float _heightToUseAerialMoves;
    [SerializeField] float _aerialSlamSpeed;
    [SerializeField] float _attackNudgeAmount;
    GameObject _player;
    GameObject _cam;
    Rigidbody _rb;
    PlayerMovement _movement;
    Animator _animator;
    GameObject _goUmbrella;
    int _attackPhase;
    Coroutine _resetAttackCoroutine;
    bool _inAttackAnimation;
    bool _inAerialAttack;

    public bool blocking;
    public InputAction blockAction;
    public UmbrellaState umbrellaState;
    public float attackTime;
    public float heightOffGround;


    void OnEnable()
    {
        var playerInput = GetComponent<PlayerInput>();
        blockAction = playerInput.actions["Block"];
        blockAction.Enable();
    }
    void Start()
    {
        _player = GameObject.FindWithTag("Player");
        _cam = GameObject.FindWithTag("MainCamera");
        _rb = _player.GetComponent<Rigidbody>();

        _movement = GetComponent<PlayerMovement>();
        _animator = _player.GetComponent<Animator>();
        Transform rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
        _goUmbrella = Instantiate(_closedUmbrella, rightHand.position, rightHand.rotation, rightHand);

        _goUmbrella.transform.localPosition = new Vector3(-0.00022f, 0.00133f, -0.00223f);
        _goUmbrella.transform.localRotation = Quaternion.Euler(-1.443f, 11.238f, -31.748f);
        _goUmbrella.transform.localScale = new Vector3(0.006f, 0.006f, 0.006f);

        umbrellaState = UmbrellaState.Closed;
    }

    void FixedUpdate()
    {
        int playerMask = ~LayerMask.GetMask("Player");
        Physics.Raycast(_player.transform.position, -_player.transform.up, out RaycastHit hit, 100, playerMask);
        heightOffGround = _player.transform.position.y - hit.point.y;
    }

    void Update()
    {
        bool canBlock = _movement.isGrounded || _rb.linearVelocity.y < 0;

        if (blockAction.IsPressed() && canBlock)
        {
            if (!_movement.isGrounded && !_movement.isAscending)
                _movement.isGliding = true;
            else
                _movement.isGliding = false;



            if (umbrellaState == UmbrellaState.Closed)
            {
                umbrellaState = UmbrellaState.Open;
                UpdateUmbrella(_openUmbrella);
                blocking = true;

                if (!_movement.isGliding)
                    _movement.AddSpeedMultiplier("umbrella", 0.5f);

                if (_resetAttackCoroutine != null)
                {
                    _attackPhase = 0;
                    _movement.canMove = true;
                    StopCoroutine(_resetAttackCoroutine);
                    _resetAttackCoroutine = null;
                    _animator.SetTrigger("ResetAttack");
                }
            }

            _movement.SetPlayerRotationToCameraRotation(Vector3.ProjectOnPlane(_cam.transform.forward, Vector3.up).normalized);
        }
        else
        {
            if (umbrellaState == UmbrellaState.Open)
            {
                umbrellaState = UmbrellaState.Closed;
                UpdateUmbrella(_closedUmbrella);
                _movement.RemoveSpeedMultiplier("umbrella");
                blocking = false;
            }

            _movement.isGliding = false;
        }


        _animator.SetBool("Gliding", _movement.isGliding);

        if (!_movement.isGliding)
            _animator.SetBool("Blocking", blocking);
        else
            _animator.SetBool("Blocking", false);
    }

    public void OnAttack()
    {
        if (umbrellaState == UmbrellaState.Open) return;

        if (_inAttackAnimation) return;

        if (heightOffGround >= _heightToUseAerialMoves)
        {
            OnAerialAttack();
            return;
        }

        _inAttackAnimation = true;

        _movement.canMove = false;
        _animator.SetBool("Walking", false);

        if (_resetAttackCoroutine != null)
        {
            StopCoroutine(_resetAttackCoroutine);
            _resetAttackCoroutine = null;
        }
        _resetAttackCoroutine = StartCoroutine(ResetMovement());

        _animator.SetTrigger("Attack" + _attackPhase);
        if (_attackPhase++ >= 2) _attackPhase = 0;
        StartCoroutine(PlayerNudge());
        StartCoroutine(WaitForAnimation());
    }

    IEnumerator PlayerNudge()
    {
        float time = 8;
        while (time > 0)
        {
            yield return new WaitForFixedUpdate();
            _rb.AddForce(_rb.transform.forward * _attackNudgeAmount, ForceMode.Acceleration);
            time--;
        }
    }

    IEnumerator ResetMovement(float duration = 0f)
    {
        yield return new WaitForSeconds(duration <= 0 ? attackTime : duration);

        _attackPhase = 0;
        _movement.canMove = true;
        _resetAttackCoroutine = null;
        _animator.SetTrigger("ResetAttack");
    }

    IEnumerator WaitForAnimation()
    {
        AnimatorClipInfo[] text = _animator.GetCurrentAnimatorClipInfo(0);
        yield return new WaitForSeconds(text[0].clip.length - 0.5f);
        _inAttackAnimation = false;
    }

    void OnAerialAttack()
    {
        if (_inAerialAttack) return;

        _inAerialAttack = true;
        _rb.linearVelocity = new Vector3(0f, -_aerialSlamSpeed, 0f);
        _animator.SetTrigger("AerialAttack");
        StartCoroutine(WaitForGround());
    }

    IEnumerator WaitForGround()
    {
        yield return new WaitUntil(() => heightOffGround <= 10f);

        _inAerialAttack = false;
        _animator.SetTrigger("AerialSlam");
        _movement.canMove = false;
        _resetAttackCoroutine = StartCoroutine(ResetMovement(1f));
    }

    public void OnParry()
    {
        if (umbrellaState == UmbrellaState.Open) return;
    }

    void UpdateUmbrella(GameObject umbrellaModel)
    {
        Transform rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
        Destroy(_goUmbrella);
        _goUmbrella = Instantiate(umbrellaModel, rightHand.position, rightHand.rotation, rightHand);

        _goUmbrella.transform.localPosition = new Vector3(-0.00022f, 0.00133f, -0.00223f);
        _goUmbrella.transform.localRotation = Quaternion.Euler(-1.443f, 11.238f, -31.748f);
        _goUmbrella.transform.localScale = new Vector3(0.006f, 0.006f, 0.006f);
    }
}
