using System.Collections.Generic;

// Configuration for decor placement
[System.Serializable]
public class DecorConfig
{
    public float MinDensity = 0.1f;
    public float MaxDensity = 0.3f;
    public bool UseMeshCombining = true;
    public Dictionary<string, float> PrefabWeights = new Dictionary<string, float>();
}