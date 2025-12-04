using UnityEngine;

public class StructureEntranceDetector : MonoBehaviour
{
    private Structure _parentStructure;
    void Start()
    {
        _parentStructure = transform.parent.GetComponent<Structure>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!_parentStructure.playerInside)
            {
                _parentStructure.CullStructures();
            }
        }
    }
}
