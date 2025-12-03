using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StructureGenerator : MonoBehaviour
{
    [SerializeField] Structure spawn;
    [SerializeField] List<Structure> structures;

    [SerializeField] int totalGenerations;
    [SerializeField] float overlapThreshold;

    void Start()
    {
        GenerateStructures(spawn.entrance, 0, -1);
    }

    void GenerateStructures(GameObject exit, int currentGeneration, int lastGeneratedStructureIndex)
    {
        if (currentGeneration >= totalGenerations) return;

        int index;

        do
        {
            index = Random.Range(0, structures.Count);
        } while (index == lastGeneratedStructureIndex);

        Structure branch = Instantiate(structures[index], gameObject.transform);
        
        branch.transform.rotation = Quaternion.LookRotation(exit.transform.forward);
        branch.transform.position = exit.transform.position;

        ClearOverlaps(exit.transform.parent, branch.transform);

        foreach (GameObject e in branch.exits)
        {
            GenerateStructures(e, currentGeneration + 1, index);
        }
    }


    void ClearOverlaps(Transform root, Transform branch)
    {
        List<Transform> rootChildren = GetRelevantChildren(root);
        List<Transform> branchChildren = GetRelevantChildren(branch);


        foreach (Transform rootChild in rootChildren)
        {
            Bounds rootBounds = GetWorldBounds(rootChild);
            foreach (Transform branchChild in branchChildren)
            {
                if (rootChild.CompareTag(branchChild.tag))
                {
                    float sqrDist = (rootChild.position - branchChild.position).sqrMagnitude;

                    if (sqrDist < overlapThreshold * overlapThreshold)
                    {
                        Destroy(branchChild.gameObject);
                        continue;
                    }

                    // Bounds branchBounds = GetWorldBounds(branchChild);

                    // if (rootBounds.Intersects(branchBounds))
                    // {
                    //     Destroy(branchChild.gameObject);
                    //     continue;
                    // }
                }
            }
        }
    }

    List<Transform> GetRelevantChildren(Transform root)
    {
        List<Transform> list = new List<Transform>();
        foreach (Transform t in root.GetComponentsInChildren<Transform>())
        {
            if (t.CompareTag("Wall") || t.CompareTag("Door"))
                list.Add(t);
        }
        return list;
    }

    Bounds GetWorldBounds(Transform t)
    {
        Renderer rend = t.GetComponent<Renderer>();
        if (rend != null)
            return rend.bounds;
        else
            return new Bounds(t.position, Vector3.zero);
    }
}
