// -------------------------------------------------- //
// Scripts/Manager/EnemyManager.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections;

public class EnemyManager : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1f);
    public GameObject enemyPrefab;
    public int maxEnemiesPerRoom = 2;
    private LevelModel currentLevel;
    
    void Start()
    {
        StartCoroutine(InitializeSpawning());
    }
    
    private IEnumerator InitializeSpawning()
    {
        yield return _waitForSeconds1;
        
        LayoutManager generator = FindObjectOfType<LayoutManager>();
        if (generator != null && generator.CurrentLayout != null)
        {
            currentLevel = generator.CurrentLayout;
            SpawnEnemiesInCombatRooms();
        }
    }
    
    private void SpawnEnemiesInCombatRooms()
    {
        if (currentLevel == null || enemyPrefab == null) return;
        
        int totalEnemies = 0;
        
        foreach (var room in currentLevel.Rooms)
            if (room.Type == RoomType.Combat && !room.IsCleared) totalEnemies += SpawnEnemiesInRoom(room);
        
        Debug.Log($"Spawned {totalEnemies} enemies across dungeon");
    }
    
    private int SpawnEnemiesInRoom(RoomModel room)
    {
        int enemyCount = Random.Range(1, maxEnemiesPerRoom + 1);
        
        for (int i = 0; i < enemyCount; i++)
        {
            Vector2Int spawnPos = room.GetRandomSpawnPosition();
            if (spawnPos != Vector2Int.zero)
            {
                Vector3 worldPos = new(spawnPos.x + 0.5f, 0, spawnPos.y + 0.5f);
                Instantiate(enemyPrefab, worldPos, Quaternion.identity, transform);
            }
        }
        
        return enemyCount;
    }
}