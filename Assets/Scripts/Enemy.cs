using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    public static event Action<Enemy> OnDeath;

    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] private BloodPickup bloodPickupPrefab;

    protected Transform player;
    protected float maxHealth;
    protected float currentHealth;
    protected float contactDamage;
    protected int currentWave;
    private int bloodDropAmount;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    
    private void Awake()
    {
        player = FindFirstObjectByType<PlayerMover>().transform;
        agent.baseOffset = 0f;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    public void Initialize(EnemyData enemyData, int currentWave)
    {
        this.currentWave = currentWave;
        var waveOffset = currentWave - enemyData.StartWave;
        maxHealth = enemyData.MaxHealth + (enemyData.HealthIncreasePerWave * waveOffset);
        currentHealth = maxHealth;
        bloodDropAmount = enemyData.BloodDropAmount;
        contactDamage = enemyData.Damage + (enemyData.DamageIncreasePerWave * waveOffset);
        agent.speed = enemyData.MoveSpeed;
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (!(currentHealth <= 0))
        {
            return;
        }
        SpawnBloodPickup(bloodDropAmount);
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }

    public void RestoreHealth(float amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
    
    private void SpawnBloodPickup(int amount)
    {
        var bloodPosition = transform.position;
        var bloodRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        BloodPickup.SpawnOrGrow(bloodPickupPrefab, bloodPosition, bloodRotation, amount);
    }
}
