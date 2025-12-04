using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class StructureGenerator : MonoBehaviour
{
    [SerializeField] Structure spawn;
    [SerializeField] List<Structure> structures;
    [SerializeField] int totalGenerations;
    [SerializeField] float overlapThreshold;

    private NavMeshSurface _navMeshSurface;

    public List<GameObject> enemies;

    void Start()
    {
        GenerateStructures(spawn.entrance, 0, -1, null);
        _navMeshSurface = GetComponent<NavMeshSurface>();
    }

    void GenerateStructures(GameObject exit, int currentGeneration, int lastGeneratedStructureIndex, List<GameObject> subsequentStructures)
    {
        if (currentGeneration >= totalGenerations)
        {
            StartCoroutine(FinalizeDungeonGeneration(subsequentStructures));
            return;
        }

        int index;

        do
        {
            index = Random.Range(0, structures.Count);
        } while (index == lastGeneratedStructureIndex);

        if (subsequentStructures == null)
            subsequentStructures = new List<GameObject>();


        Structure branch = Instantiate(structures[index], gameObject.transform);
        branch.transform.rotation = Quaternion.LookRotation(exit.transform.forward);
        branch.transform.position = exit.transform.position;

        branch.Initialize(this);

        Structure root = exit.transform.parent.GetComponent<Structure>();
        root.neighbors.Add(branch);
        branch.neighbors.Add(root);


        if (currentGeneration != 0)
        {
            subsequentStructures.Add(branch.gameObject);
        }

        ClearOverlaps(exit.transform.parent, branch.transform);

        foreach (GameObject e in branch.exits)
        {
            GenerateStructures(e, currentGeneration + 1, index, subsequentStructures);
        }
    }

    public IEnumerator FinalizeDungeonGeneration(List<GameObject> subsequentStructures)
    {
        yield return new WaitForEndOfFrame();

        _navMeshSurface.BuildNavMesh();

        yield return new WaitForEndOfFrame();
        
        foreach (GameObject structure in subsequentStructures)
        {
            structure.SetActive(false);
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
