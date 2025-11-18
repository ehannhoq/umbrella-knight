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
    GameObject _player;
    Rigidbody _rb;
    PlayerMovement _movement;
    Animator _animator;
    GameObject _goUmbrella;
    [SerializeField] bool _canUseAerialMoves;
    Action _attacked;
    int _attackPhase;
    Coroutine _resetAttackCoroutine;

    public InputAction blockAction;
    public UmbrellaState umbrellaState;
    public float attackTime;


    void OnEnable()
    {
        var playerInput = GetComponent<PlayerInput>();
        blockAction = playerInput.actions["Block"];
        blockAction.Enable();
    }
    void Start()
    {
        _player = GameObject.FindWithTag("Player");
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
        _canUseAerialMoves = !Physics.Raycast(_player.transform.position, -_player.transform.up, _heightToUseAerialMoves, playerMask);
    }

    void Update()
    {
        if (blockAction.IsPressed())
        {
            if (umbrellaState == UmbrellaState.Closed)
            {
                umbrellaState = UmbrellaState.Open;
                UpdateUmbrella(_openUmbrella);
                _movement.AddSpeedMultiplier("umbrella", 0.5f);
            }
        }
        else
        {
            if (umbrellaState == UmbrellaState.Open)
            {
                umbrellaState = UmbrellaState.Closed;
                UpdateUmbrella(_closedUmbrella);
                _movement.RemoveSpeedMultiplier("umbrella");
            }
        }
    }

    public void OnAttack()
    {       
        if (umbrellaState == UmbrellaState.Open) return;

        _movement.canMove = false;


        
        if (_canUseAerialMoves)
        {
            OnAerialAttack();
            return;
        }

        _animator.SetTrigger("Attack" + _attackPhase);
        if (_attackPhase++ >= 3) _attackPhase = 0;
        _animator.SetBool("Walking", false);
        if (_resetAttackCoroutine != null)
        {
            StopCoroutine(_resetAttackCoroutine);
            _resetAttackCoroutine = null;
        }

        _resetAttackCoroutine = StartCoroutine(ResetMovement());

    }

    IEnumerator ResetMovement()
    {
        yield return new WaitForSeconds(attackTime);

        _attackPhase = 0;
        _movement.canMove = true;
        _resetAttackCoroutine = null;
        _animator.SetTrigger("ResetAttack");
    }

    void OnAerialAttack()
    {
        _rb.linearVelocity = new Vector3(0f, -_aerialSlamSpeed, 0f);
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
