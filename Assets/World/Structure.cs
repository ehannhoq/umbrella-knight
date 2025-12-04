using System;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour
{
    public GameObject entrance;
    public List<GameObject> exits;
    public List<GameObject> spawnAnchors;
    public bool clearedRoom;

    public bool playerInside;

    public List<Structure> neighbors;
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

    public void CullStructures()
    {
        playerInside = true;

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
