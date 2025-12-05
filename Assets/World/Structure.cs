using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Structure : MonoBehaviour
{
    public bool clearedRoom;
    public bool playerInside;
    public GameObject entrance;
    public List<GameObject> exits;

    public List<Structure> neighbors;

    public StructureGenerator dungeonGenerator;
    public List<Transform> enemySpawns;

    public void Initialize(StructureGenerator dungeonGenerator)
    {
        this.dungeonGenerator = dungeonGenerator;
        neighbors = new List<Structure>();
        enemySpawns = new List<Transform>();

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
        if (clearedRoom)
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
        SpawnEnemies();
        CullStructures();
    }

    void SpawnEnemies()
    {
        if (enemySpawns.Count <= 0) return;

        int randomEnemyIndex = Random.Range(0, dungeonGenerator.enemies.Count - 1);
        int randomSpawnIndex = Random.Range(0, enemySpawns.Count - 1);

        EnemyAI enemy = dungeonGenerator.enemies[randomEnemyIndex];
        Transform spawnPos = enemySpawns[randomSpawnIndex];

        enemy.Spawn(spawnPos);
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
