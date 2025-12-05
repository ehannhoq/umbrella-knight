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

    public List<EnemyAI> enemies;

    [Header("Dead-end bias")]
    [Tooltip("If remaining generations <= this threshold, prefer structures with 0 exits when a parent has fewer neighbors than exits.")]
    [SerializeField] int deadEndPriorityThreshold = 2;
    [Tooltip("Multiplier applied to spawnWeight for zero-exit structures when bias applies.")]
    [SerializeField] int deadEndWeightMultiplier = 4;
    [Tooltip("Minimum generation index before dead-end bias can apply. Prevents biasing toward dead-ends at very early generations.")]
    [SerializeField] int deadEndMinGeneration = 1;

    void Start()
    {
        _navMeshSurface = GetComponent<NavMeshSurface>();
        GenerateStructuresIterative(spawn.entrance);
    }

    void GenerateStructuresIterative(GameObject startExit)
    {
        var subsequentStructures = new List<GameObject>();

        var q = new Queue<(GameObject exit, int generation, int lastGeneratedStructureIndex)>();
        q.Enqueue((startExit, 0, -1));

        while (q.Count > 0)
        {
            var item = q.Dequeue();
            GameObject exit = item.exit;
            int currentGeneration = item.generation;
            int lastGeneratedStructureIndex = item.lastGeneratedStructureIndex;

            if (exit == null) continue;

            if (currentGeneration >= totalGenerations)
            {
                continue;
            }

            int remaining = totalGenerations - currentGeneration;
            Structure parentStruct = exit.transform.parent.GetComponent<Structure>();

            List<int> weights = new List<int>(structures.Count);
            for (int i = 0; i < structures.Count; i++)
            {
                int weight = Mathf.Max(1, structures[i].spawnWeight);

                if (parentStruct.neighbors.Count < parentStruct.exits.Count && remaining <= deadEndPriorityThreshold)
                {
                    if (structures[i].exits.Count == 0)
                    {
                        if (currentGeneration >= deadEndPriorityThreshold)
                            weight *= deadEndWeightMultiplier;
                        else if (currentGeneration <= deadEndMinGeneration)
                            weight = 0;
                    }
                }

                if (i == lastGeneratedStructureIndex)
                    weight = 0;

                weights.Add(weight);
            }

            int totalWeight = 0;
            for (int i = 0; i < weights.Count; i++) totalWeight += weights[i];

            int index = 0;
            if (totalWeight <= 0)
            {
                index = (lastGeneratedStructureIndex + 1) % Mathf.Max(1, structures.Count);
            }
            else
            {
                int rand = Random.Range(0, totalWeight);
                int acc = 0;
                for (int i = 0; i < weights.Count; i++)
                {
                    acc += weights[i];
                    if (rand < acc)
                    {
                        index = i;
                        break;
                    }
                }
            }

            Structure branch = Instantiate(structures[index], gameObject.transform);
            branch.transform.rotation = Quaternion.LookRotation(exit.transform.forward);
            branch.transform.position = exit.transform.position;
            branch.clearedRoom = false;
            branch.Initialize(this);

            if (parentStruct != null)
            {
                parentStruct.neighbors.Add(branch);
                branch.neighbors.Add(parentStruct);
            }

            if (currentGeneration != 0)
            {
                subsequentStructures.Add(branch.gameObject);
            }

            ClearOverlaps(exit.transform.parent, branch.transform);

            foreach (GameObject e in branch.exits)
            {
                q.Enqueue((e, currentGeneration + 1, index));
            }
        }

        StartCoroutine(FinalizeDungeonGeneration(subsequentStructures));
    }

    public IEnumerator FinalizeDungeonGeneration(List<GameObject> subsequentStructures)
    {
        yield return new WaitForEndOfFrame();

        _navMeshSurface.BuildNavMesh();

        yield return new WaitForEndOfFrame();

        spawn.neighbors[0].entrance.GetComponent<Door>().locked = false;

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
