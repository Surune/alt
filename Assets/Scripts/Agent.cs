using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Agent : MonoBehaviour
{
    public static event Action<Agent> Died;

    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected int maxHealth = 3;
    [SerializeField] private BloodPickup bloodPickupPrefab;

    protected int currentHealth;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Initialize(int health)
    {
        maxHealth = health;
        currentHealth = health;
    }

    public void TakeDamage(int damage)
    {
        SpawnBloodPickup();
    
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Died?.Invoke(this);
            Destroy(gameObject);
        }
    }
    
    private void SpawnBloodPickup()
    {
        var bloodPosition = transform.position;
        bloodPosition.y = 0.02f;
        var bloodRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        BloodPickup.SpawnOrGrow(bloodPickupPrefab, bloodPosition, bloodRotation);
    }
}
