// -------------------- //
// Scripts/Commons/Interfaces/IEntity.cs
// -------------------- //

using UnityEngine;

public interface IEntity : IDamageable
{
    public bool IsAlive { get; set; }
    public bool IsActive { get; set; }
}

public interface IDamageable
{
    public int Health { get; set; }

    public void TakeDmg(int dmg)
    {
        Health = Mathf.Max(Health - dmg, 0);
        Debug.Log($"{GetType().Name} took {dmg} damage.");
        if (Health == 0) Die();
    }

    public void Die();
}