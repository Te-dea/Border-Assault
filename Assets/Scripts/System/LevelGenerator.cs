using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("生成设置")]
    public int maxRooms = 8;
    public Vector2Int roomSizeRange = new Vector2Int(8, 12);
    public Room[] roomPrefabs;
    public Transform player;

    private List<Vector2Int> roomPositions = new List<Vector2Int>();
    private List<GameObject> spawnedRooms = new List<GameObject>();

    public void GenerateLevel()
    {
        ClearLevel();
        roomPositions.Clear();

        // 生成起始房间
        Vector2Int startPos = Vector2Int.zero;
        roomPositions.Add(startPos);
        SpawnRoom(startPos);

        // 生成其他房间
        for (int i = 1; i < maxRooms; i++)
        {
            Vector2Int newPos = GetRandomAdjacentPosition();
            roomPositions.Add(newPos);
            SpawnRoom(newPos);
        }

        // 放置玩家
        player.position = new Vector3(startPos.x * roomSizeRange.x, startPos.y * roomSizeRange.y, 0);
    }

    private Vector2Int GetRandomAdjacentPosition()
    {
        Vector2Int pos = Vector2Int.zero;
        int attempts = 0;

        do
        {
            int randomIndex = Random.Range(0, roomPositions.Count);
            Vector2Int basePos = roomPositions[randomIndex];
            int direction = Random.Range(0, 4);

            pos = direction switch
            {
                0 => basePos + Vector2Int.up,
                1 => basePos + Vector2Int.right,
                2 => basePos + Vector2Int.down,
                _ => basePos + Vector2Int.left
            };

            attempts++;
        } while (roomPositions.Contains(pos) && attempts < 100);

        return pos;
    }

    private void SpawnRoom(Vector2Int gridPos)
    {
        Room randomRoom = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
        Vector3 worldPos = new Vector3(
            gridPos.x * roomSizeRange.x,
            gridPos.y * roomSizeRange.y,
            0
        );

        GameObject room = Instantiate(randomRoom.prefab, worldPos, Quaternion.identity, transform);
        spawnedRooms.Add(room);
    }

    private void ClearLevel()
    {
        foreach (var room in spawnedRooms)
        {
            Destroy(room);
        }
        spawnedRooms.Clear();
    }
}
