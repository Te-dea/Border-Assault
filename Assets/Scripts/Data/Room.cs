using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Room", menuName = "PixelRogue/Room")]
public class Room : ScriptableObject
{
    public Vector2Int size = new Vector2Int(10, 6);
    public bool[] doors = new bool[4]; // иосробвС
    public GameObject prefab;
}