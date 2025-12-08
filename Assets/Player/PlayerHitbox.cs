using Unity.VisualScripting;
using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {        
        if (other.TryGetComponent(out Projectile projectile))
            PlayerStats.Instance.DealDamage(projectile.damage);
    }
}
