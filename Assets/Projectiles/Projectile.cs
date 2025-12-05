using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum WallBehavior
    {
        DieOnImpact,
        Bounce
    }

    public float speed;
    public float damage;
    public WallBehavior wallBehavior;
    public int numBounces = 1;

    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        _rb.linearVelocity = transform.forward * speed;
        _rb.freezeRotation = true;

        OnSpawn();
    }

    void FixedUpdate()
    {
        if (Physics.Raycast(
            _rb.position,
            _rb.transform.forward,
            out RaycastHit hit,
            _rb.linearVelocity.magnitude * Time.fixedDeltaTime,
            Util.nonColliderMasks & ~LayerMask.GetMask("NPC")
        ))
        {
            switch (wallBehavior) 
            {
                case WallBehavior.DieOnImpact:
                    OnDestroy();
                    break;
                case WallBehavior.Bounce:
                    if (numBounces <= 1)
                    {
                        wallBehavior = WallBehavior.DieOnImpact;
                    }

                    _rb.linearVelocity = Vector3.Reflect(_rb.linearVelocity, hit.normal);
                    numBounces--;
                    break;
            }
        }

        if (_rb.linearVelocity.magnitude < speed)
            OnDestroy();
    }

    public void SpawnProjectile(Transform spawnPos, Vector3 direction)
    {
        Instantiate(this, spawnPos.position, spawnPos.rotation);
    }

    public void SpawnProjectile(Vector3 position, Quaternion rotation)
    {
        Instantiate(this, position, rotation);
    }

    public virtual void OnSpawn() { }

    public virtual void OnDestroy()
    {
        Destroy(gameObject);
    }
}