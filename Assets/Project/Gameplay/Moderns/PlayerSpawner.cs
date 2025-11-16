using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Player Prefab")]
    public GameObject playerPrefab;
    
    [Header("Camera")]
    public CameraFollow cameraFollow;
    
    private GameObject currentPlayer;
    
    public void OnDungeonGenerated()
    {
        SpawnPlayer();
    }
    
    private void SpawnPlayer()
    {
        // Remove existing player if any
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }
        
        if (playerPrefab != null)
        {
            // Get spawn position from dungeon generator
            DungeonGenerator generator = GetComponent<DungeonGenerator>();
            Vector3 spawnPosition = generator.GetEntranceRoomPosition();
            
            // Instantiate player
            currentPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            
            // Setup camera follow
            if (cameraFollow != null)
            {
                cameraFollow.SetTarget(currentPlayer.transform);
            }
            
            Debug.Log($"Player spawned at entrance: {spawnPosition}");
        }
        else
        {
            Debug.LogError("Player prefab not assigned in PlayerSpawner!");
        }
    }
    
    public GameObject GetCurrentPlayer()
    {
        return currentPlayer;
    }
}