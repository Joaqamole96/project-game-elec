// -------------------------------------------------- //
// Scripts/Managers/PlayerManager.cs
// -------------------------------------------------- //

using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public CameraController cameraController;
    
    private GameObject currentPlayer;
    
    public void OnDungeonGenerated() =>  SpawnPlayer();
    
    private void SpawnPlayer()
    {
        if (currentPlayer != null) Destroy(currentPlayer);
        
        if (playerPrefab != null)
        {
            LayoutManager generator = GetComponent<LayoutManager>();
            Vector3 spawnPosition = generator.GetEntranceRoomPosition();
            
            currentPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            
            if (cameraController != null) cameraController.SetTarget(currentPlayer.transform);
        }
    }
}