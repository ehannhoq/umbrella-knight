using System;
using System.Collections;
using System.Collections.Generic;
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
    Collider _collider;
    Coroutine _blockDelayCoroutine;
    HashSet<EnemyAI> _hitEnemiesThisAttack;

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

        _collider = _goUmbrella.GetComponent<Collider>();
        _collider.enabled = false;

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
        bool canBlock = _movement.grounded || _rb.linearVelocity.y < 0;

        if (blockAction.IsPressed())
        {
            _collider.enabled = false;

            if (!_movement.grounded && !_movement.ascending)
            {
                _movement.gliding = true;
            }
            else
            {
                _movement.gliding = false;
                StartCoroutine(Util.DelayedActionSeconds(0.25f, () => { _collider.enabled = true; }));
            }


            if (umbrellaState == UmbrellaState.Closed)
            {
                umbrellaState = UmbrellaState.Open;
                UpdateUmbrella(_openUmbrella);
                blocking = true;

                if (!_movement.gliding)
                    // _movement.AddSpeedMultiplier("umbrella", 0.5f);

                    if (_resetAttackCoroutine != null)
                    {
                        _attackPhase = 0;
                        _movement.canMove = true;
                        StopCoroutine(_resetAttackCoroutine);
                        _resetAttackCoroutine = null;
                        _animator.SetTrigger("ResetAttack");
                    }
            }

            _movement.RotatePlayer(Vector3.ProjectOnPlane(_cam.transform.forward, Vector3.up).normalized);
        }
        else
        {
            if (umbrellaState == UmbrellaState.Open)
            {
                umbrellaState = UmbrellaState.Closed;
                UpdateUmbrella(_closedUmbrella);
                // _movement.RemoveSpeedMultiplier("umbrella");
                blocking = false;
            }

            _movement.gliding = false;
        }


        _animator.SetBool("Gliding", _movement.gliding);
        if (_blockDelayCoroutine == null)
            _blockDelayCoroutine = StartCoroutine(Util.DelayedActionEndOfFrame(() => { 
                _animator.SetBool("Blocking", blocking);
                _blockDelayCoroutine = null;
            }));
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
        _animator.SetBool("Moving", false);
        _collider.enabled = true;

        // start fresh hit tracking for this attack and do an immediate overlap check
        _hitEnemiesThisAttack = new HashSet<EnemyAI>();
        DoHitCheck();
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

            // check overlap each physics step to catch fast/mid-frame collisions
            DoHitCheck();
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
        _collider.enabled = false;

        // clear hit tracking when the attack finishes
        _hitEnemiesThisAttack = null;
    }

    // Physics overlap check to reliably detect enemies hit by the umbrella.
    void DoHitCheck()
    {
        if (_collider == null) return;
        if (_hitEnemiesThisAttack == null) _hitEnemiesThisAttack = new HashSet<EnemyAI>();

        // Use the collider's bounds as the overlap box. Rotation is provided from the umbrella transform.
        var center = _collider.bounds.center;
        var extents = _collider.bounds.extents;
        var rot = _goUmbrella != null ? _goUmbrella.transform.rotation : Quaternion.identity;

        Collider[] hits = Physics.OverlapBox(center, extents, rot);
        foreach (var c in hits)
        {
            if (c == null) continue;
            var enemy = c.GetComponentInParent<EnemyAI>();
            if (enemy == null) continue;
            if (_hitEnemiesThisAttack.Contains(enemy)) continue;

            _hitEnemiesThisAttack.Add(enemy);

            // apply damage and knockback similar to EnemyHitbox
            enemy.DealDamage(PlayerStats.Instance.attackDamage);
            if (enemy.takesKnockback)
            {
                Vector3 knockDir = (_player.transform.forward + Vector3.up).normalized;
                enemy.ChangeVelocity(knockDir * PlayerStats.Instance.knockback);
            }
        }
    }

    IEnumerator WaitForAnimation()
    {
        AnimatorClipInfo[] text = _animator.GetCurrentAnimatorClipInfo(0);
        yield return new WaitForSeconds(text[0].clip.length - 0.5f);
        _inAttackAnimation = false;
        _collider.enabled = false;
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

        _collider = _goUmbrella.GetComponent<Collider>();
        _collider.enabled = false;
    }
}
