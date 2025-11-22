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
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }
        
        if (playerPrefab != null)
        {
            LayoutManager generator = GetComponent<LayoutManager>();
            Vector3 spawnPosition = generator.GetEntranceRoomPosition();
            
            currentPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            
            if (cameraFollow != null)
            {
                cameraFollow.SetTarget(currentPlayer.transform);
            }
        }
    }
    
    public GameObject GetCurrentPlayer()
    {
        return currentPlayer;
    }
}