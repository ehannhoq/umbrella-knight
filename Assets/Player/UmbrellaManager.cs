using System;
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
    Action attacked;

    public InputAction blockAction;
    public UmbrellaState umbrellaState;


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

        _goUmbrella.transform.localPosition = new Vector3(-0.015f, -0.022f, 0.007f);
        _goUmbrella.transform.localRotation = Quaternion.Euler(52.053f, -58.615f, -58.528f);
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
        
        if (_canUseAerialMoves)
        {
            OnAerialAttack();
            return;
        }

        System.Random rand = new System.Random();
        _animator.SetTrigger("Attack" + rand.Next(1, 4));
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

        _goUmbrella.transform.localPosition = new Vector3(-0.015f, -0.022f, 0.007f);
        _goUmbrella.transform.localRotation = Quaternion.Euler(52.053f, -58.615f, -58.528f);
        _goUmbrella.transform.localScale = new Vector3(0.006f, 0.006f, 0.006f);
    }
}
