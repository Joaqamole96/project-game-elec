// ================================================== //
// Scripts/Enemies/EnemyGenerator.cs (UPDATED)
// ================================================== //

using UnityEngine;

public static class EnemyGenerator
{
    public static GameObject CreateMeleeEnemy(Vector3 position)
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.transform.position = position;
        enemy.name = "MeleeEnemy";
        enemy.tag = "Enemy";
        
        // Visual
        Renderer renderer = enemy.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.red;
        renderer.material = mat;
        
        // Components
        UnityEngine.AI.NavMeshAgent agent = enemy.AddComponent<UnityEngine.AI.NavMeshAgent>();
        MeleeEnemyController meleeScript = enemy.AddComponent<MeleeEnemyController>();
        meleeScript.agent = agent;
        
        return enemy;
    }
    
    public static GameObject CreateRangedEnemy(Vector3 position)
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.transform.position = position;
        enemy.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        enemy.name = "RangedEnemy";
        enemy.tag = "Enemy";
        
        // Visual
        Renderer renderer = enemy.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.yellow;
        renderer.material = mat;
        
        // Components
        UnityEngine.AI.NavMeshAgent agent = enemy.AddComponent<UnityEngine.AI.NavMeshAgent>();
        RangedEnemyController rangedScript = enemy.AddComponent<RangedEnemyController>();
        rangedScript.agent = agent;
        
        return enemy;
    }
    
    public static GameObject CreateTankEnemy(Vector3 position)
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.transform.position = position;
        enemy.transform.localScale = new Vector3(1.5f, 1.2f, 1.5f);
        enemy.name = "TankEnemy";
        enemy.tag = "Enemy";
        
        // Visual
        Renderer renderer = enemy.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.3f, 0.3f, 0.3f);
        renderer.material = mat;
        
        // Components
        UnityEngine.AI.NavMeshAgent agent = enemy.AddComponent<UnityEngine.AI.NavMeshAgent>();
        TankEnemyController tankScript = enemy.AddComponent<TankEnemyController>();
        tankScript.agent = agent;
        
        return enemy;
    }
}