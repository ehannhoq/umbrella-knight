using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "ChestLoot", menuName = "Scriptable Objects/ChestLoot")]
public class ChestLoot : ScriptableObject
{
    [Serializable]
    public struct Loot
    {
        public Item item;
        public float weight;
    }

    public List<Loot> loot;
    
    public Item Roll()
    {
        return null;
    }
}
