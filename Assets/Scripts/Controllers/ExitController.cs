// ================================================== //
// Scripts/Controllers/ExitController.cs (COMPLETE REWRITE)
// ================================================== //

using UnityEngine;

/// <summary>
/// Exit portal controller - properly triggers floor transition
/// Works with ProgressionManager to advance floors
/// </summary>
[RequireComponent(typeof(Collider))]
public class ExitController : MonoBehaviour
{
    [Header("Visual Effects")]
    public float rotationSpeed = 50f;
    public GameObject visualEffect;
    public Renderer portalRenderer;
    
    [Header("State")]
    public bool isActive = true;
    private bool hasBeenUsed = false;
    
    [Header("Colors")]
    public Color activeColor = Color.cyan;
    public Color inactiveColor = Color.gray;
    
    void Start()
    {
        SetupCollider();
        SetupVisuals();
        
        Debug.Log("ExitController: Portal initialized");
    }
    
    void Update()
    {
        // Rotate for visual effect
        if (visualEffect != null)
        {
            visualEffect.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        
        // Pulsate color
        if (portalRenderer != null && isActive)
        {
            float pulse = Mathf.PingPong(Time.time * 2f, 1f);
            Color color = Color.Lerp(activeColor, activeColor * 1.5f, pulse);
            
            Material mat = portalRenderer.material;
            mat.SetColor("_EmissionColor", color * 2f);
        }
    }
    
    private void SetupCollider()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = 2f;
            Debug.Log("ExitController: Created collider");
        }
        else
        {
            col.isTrigger = true;
            Debug.Log("ExitController: Using existing collider");
        }
    }
    
    private void SetupVisuals()
    {
        // Get renderer
        if (portalRenderer == null)
        {
            portalRenderer = GetComponentInChildren<Renderer>();
        }
        
        if (portalRenderer != null)
        {
            Material mat = portalRenderer.material;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", activeColor * 2f);
        }
        
        // Setup visual effect if exists
        if (visualEffect == null)
        {
            // Try to find child named "Effect" or "Visual"
            Transform effectTransform = transform.Find("Effect");
            if (effectTransform == null)
            {
                effectTransform = transform.Find("Visual");
            }
            
            if (effectTransform != null)
            {
                visualEffect = effectTransform.gameObject;
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!isActive || hasBeenUsed)
        {
            return;
        }
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("ExitController: Player entered portal!");
            hasBeenUsed = true;
            
            UsePortal();
        }
    }
    
    private void UsePortal()
    {
        Debug.Log("ExitController: Activating portal...");
        
        // Find ProgressionManager
        ProgressionManager progressionManager = FindObjectOfType<ProgressionManager>();
        
        if (progressionManager != null)
        {
            Debug.Log("ExitController: Calling ProgressionManager.OnPlayerEnterExitPortal()");
            progressionManager.OnPlayerEnterExitPortal();
        }
        else
        {
            Debug.LogError("ExitController: ProgressionManager not found! Creating one...");
            
            // Create ProgressionManager if it doesn't exist
            GameObject managerObj = new GameObject("ProgressionManager");
            progressionManager = managerObj.AddComponent<ProgressionManager>();
            
            // Try again
            progressionManager.OnPlayerEnterExitPortal();
        }
    }
    
    public void SetActive(bool active)
    {
        isActive = active;
        
        if (portalRenderer != null)
        {
            Material mat = portalRenderer.material;
            Color color = active ? activeColor : inactiveColor;
            mat.color = color;
            mat.SetColor("_EmissionColor", color * (active ? 2f : 0.5f));
        }
        
        Debug.Log($"ExitController: Portal {(active ? "activated" : "deactivated")}");
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = isActive ? Color.cyan : Color.gray;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}

