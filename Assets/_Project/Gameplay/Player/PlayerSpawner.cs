using System.Linq;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Player Prefab")]
    public GameObject PlayerPrefab;
    
    [Header("References")]
    public DungeonGenerator DungeonGenerator;
    
    [Header("Spawn Settings")]
    public float SpawnHeight = 1f;
    
    private GameObject _currentPlayer;

    void Start()
    {
        if (DungeonGenerator != null && DungeonGenerator.CurrentLayout != null)
        {
            SpawnPlayer();
        }
    }

    [ContextMenu("Spawn Player")]
    public void SpawnPlayer()
    {
        if (DungeonGenerator == null)
        {
            Debug.LogError("DungeonGenerator reference not set!");
            return;
        }

        // Remove existing player
        if (_currentPlayer != null)
        {
            DestroyImmediate(_currentPlayer);
        }

        // Get spawn position
        Vector3 spawnPosition = GetSpawnPosition();
        
        Debug.Log($"Attempting to spawn player at: {spawnPosition}");

        // Spawn player
        if (PlayerPrefab != null)
        {
            _currentPlayer = Instantiate(PlayerPrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"Player spawned from prefab at: {spawnPosition}");
        }
        else
        {
            _currentPlayer = CreatePlayerController(spawnPosition);
            Debug.Log($"Player controller created at: {spawnPosition}");
        }
    }

    private Vector3 GetSpawnPosition()
    {
        Vector3 entrancePos = DungeonGenerator.GetEntranceRoomPosition();
        
        if (entrancePos != Vector3.zero)
        {
            return entrancePos;
        }

        // Fallback logic
        Debug.LogWarning("No entrance found, searching for any floor tile...");
        var layout = DungeonGenerator.CurrentLayout;
        if (layout?.AllFloorTiles != null && layout.AllFloorTiles.Count > 0)
        {
            var firstFloorTile = layout.AllFloorTiles.First();
            Vector3 fallbackPos = new Vector3(firstFloorTile.x + 0.5f, SpawnHeight, firstFloorTile.y + 0.5f);
            Debug.Log($"Using fallback position: {fallbackPos}");
            return fallbackPos;
        }

        Debug.LogError("No floor tiles found! Using default position.");
        return new Vector3(0, SpawnHeight, 0);
    }

    private GameObject CreatePlayerController(Vector3 position)
    {
        // Create player object
        GameObject player = new GameObject("Player");
        player.transform.position = position;

        // Add CharacterController
        var characterController = player.AddComponent<CharacterController>();
        characterController.height = 2f;
        characterController.radius = 0.3f;
        characterController.center = new Vector3(0, 1f, 0);

        // Create camera parent
        GameObject cameraParent = new GameObject("CameraParent");
        cameraParent.transform.SetParent(player.transform);
        cameraParent.transform.localPosition = new Vector3(0, 1.6f, 0);

        // Create camera
        GameObject cameraObj = new GameObject("FirstPersonCamera");
        cameraObj.transform.SetParent(cameraParent.transform);
        cameraObj.transform.localPosition = Vector3.zero;
        cameraObj.transform.localRotation = Quaternion.identity;
        
        Camera camera = cameraObj.AddComponent<Camera>();
        cameraObj.AddComponent<AudioListener>();

        // Create ground check
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(player.transform);
        groundCheck.transform.localPosition = new Vector3(0, -0.9f, 0);

        // Add the controller component
        var controller = player.AddComponent<PlayerController>();
        
        // Assign references
        controller.playerCamera = cameraObj.transform;
        controller.cameraParent = cameraParent.transform;
        controller.groundCheck = groundCheck.transform;
        controller.groundMask = 1; // Default layer

        return player;
    }

    [ContextMenu("Respawn Player")]
    public void RespawnPlayer()
    {
        SpawnPlayer();
    }

    public void OnDungeonGenerated()
    {
        SpawnPlayer();
    }
}