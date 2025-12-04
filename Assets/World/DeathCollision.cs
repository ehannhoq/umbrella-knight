using UnityEngine;

public class DeathCollision : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Killed Player!");
        }
    }
}
