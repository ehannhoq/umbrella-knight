using UnityEngine;

public class UmbrellaBlockCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent(out Projectile projectile))
        {
            if (projectile.wallBehavior == Projectile.WallBehavior.Bounce)
            {
                if (projectile.numBounces <= 1)
                {
                    projectile.wallBehavior = Projectile.WallBehavior.DieOnImpact;
                }

                projectile.Reflect(transform.forward);
                projectile.numBounces--;

            }
            else
            {
                projectile.OnDestroy();
            }
        }
    }
}