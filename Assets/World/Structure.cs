using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Structure : MonoBehaviour
{
    public bool clearedRoom;
    public bool playerInside;
    public GameObject entrance;
    public List<GameObject> exits;
    public int spawnWeight = 1;

    public List<Structure> neighbors;

    public StructureGenerator structureGenerator;
    public List<Transform> enemySpawns;


    private List<GameObject> _activeEnemies;

    public void Initialize(StructureGenerator structureGenerator)
    {
        this.structureGenerator = structureGenerator;
        neighbors = new List<Structure>();
        enemySpawns = new List<Transform>();
        _activeEnemies = new List<GameObject>();

        foreach (Transform child in gameObject.transform)
        {
            if (child.CompareTag("EnemySpawn"))
            {
                enemySpawns.Add(child);
            }
        }
    }

    void Update()
    {
        if (!clearedRoom)
            CheckCleared();
        else
        {
            foreach (GameObject exit in exits)
            {
                if (exit.TryGetComponent(out Door door))
                {
                    door.locked = false;
                }
            }
        }
    }

    public void OnEnterStructure()
    {
        playerInside = true;
        CullStructures();

        if (!clearedRoom)
            SpawnEnemies();
    }

    void SpawnEnemies()
    {
        if (enemySpawns.Count <= 0) return;
        if (_activeEnemies.Count > 0) return;

        foreach (Transform spawn in enemySpawns)
        {
            int randomEnemyIndex = Random.Range(0, structureGenerator.enemies.Count - 1);
            EnemyAI enemyPrefab = structureGenerator.enemies[randomEnemyIndex];
            EnemyAI spawned = enemyPrefab.Spawn(spawn);
            _activeEnemies.Add(spawned.gameObject);
        }
    }

    void CheckCleared()
    {
        if (!playerInside) return;

        if (_activeEnemies == null) return;

        _activeEnemies.RemoveAll(e => e == null);

        if (_activeEnemies.Count == 0)
        {
            clearedRoom = true;
            foreach (GameObject exit in exits)
            {
                if (exit.TryGetComponent(out Door door))
                {
                    door.ToggleLocked();
                }

            }
        }
    }

    void CullStructures()
    {
        foreach (Structure neighbor in neighbors)
        {
            neighbor.gameObject.SetActive(true);
            neighbor.playerInside = false;

            foreach (Structure neighborsNeighbor in neighbor.neighbors)
            {
                if (neighborsNeighbor == this || neighbors.Contains(neighborsNeighbor)) continue;
                neighborsNeighbor.playerInside = false;
                neighborsNeighbor.gameObject.SetActive(false);
            }
        }
    }
}
