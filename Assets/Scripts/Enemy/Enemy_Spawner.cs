using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy_Spawner : Enemy_Baby
{
    [SerializeField] private EnemyData junkieData;
    [SerializeField] private int spawnCount = 3;
    [SerializeField] private float spawnRadius = 1.25f;
    [SerializeField] private float navMeshSampleDistance = 3f;

    public override void TakeDamage(float damage)
    {
        PlayDamageFlash();
        currentHealth -= damage;

        if (!(currentHealth <= 0))
        {
            return;
        }

        SpawnJunkies();
        Die();
    }

    private void SpawnJunkies()
    {
        for (var i = 0; i < spawnCount; i++)
        {
            var angle = (i / (float)spawnCount) * Mathf.PI * 2f;
            var spawnOffset = Vector3.zero;
            spawnOffset.x = Mathf.Cos(angle) * spawnRadius;
            spawnOffset.z = Mathf.Sin(angle) * spawnRadius;

            var spawnPosition = transform.position + spawnOffset;
            if (NavMesh.SamplePosition(spawnPosition, out var hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                spawnPosition = hit.position;
            }

            var enemy = Instantiate(junkieData.Prefab, spawnPosition, Quaternion.identity);
            enemy.Initialize(junkieData, currentWave);
            NotifySpawned(enemy);
        }
    }
}
