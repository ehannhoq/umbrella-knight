using UnityEngine;

public class DeathCollision : MonoBehaviour
{
    public Structure root;

    void Start()
    {
        root = FindRoot(transform);
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerStats.Instance.DealDamage(15);
        other.transform.position = root.transform.position;
    }

    private Structure FindRoot(Transform t)
    {
        if (t.TryGetComponent(out Structure s))
            return s;

        if (t.parent == null)
            return null;

        return FindRoot(t.parent);
    }
}
