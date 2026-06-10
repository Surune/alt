using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public sealed class PoolManager
{
    private sealed class PoolCollection<T> where T : Component
    {
        private readonly Dictionary<T, ObjectPool<T>> pools = new();
        private readonly Dictionary<T, ObjectPool<T>> activePools = new();

        public T Get(T prefab, Vector3 position, Quaternion rotation)
        {
            if (!pools.TryGetValue(prefab, out var pool))
            {
                pool = new ObjectPool<T>(
                    () => Object.Instantiate(prefab),
                    null,
                    instance => instance.gameObject.SetActive(false),
                    instance => Object.Destroy(instance.gameObject));
                pools.Add(prefab, pool);
            }

            var instance = pool.Get();
            instance.transform.SetPositionAndRotation(position, rotation);
            activePools.Add(instance, pool);
            return instance;
        }

        public void Release(T instance)
        {
            var pool = activePools[instance];
            activePools.Remove(instance);
            pool.Release(instance);
        }

        public void Clear()
        {
            foreach (var pool in pools.Values)
            {
                pool.Clear();
            }

            pools.Clear();
            activePools.Clear();
        }
    }

    private readonly PoolCollection<BloodPickup> bloodPickupPool = new();
    private readonly PoolCollection<PlayerProjectile> bulletPool = new();
    private readonly PoolCollection<EnemyProjectile> enemyShotPool = new();

    private BloodPickup bloodPrefab;
    private PlayerProjectile playerProjectile;
    private EnemyProjectile enemyProjectile;
    
    public PoolManager(BloodPickup bloodPrefab, PlayerProjectile playerProjectile, EnemyProjectile enemyProjectile)
    {
        this.bloodPrefab = bloodPrefab;
        this.playerProjectile = playerProjectile;
        this.enemyProjectile = enemyProjectile;
    }
    
    public BloodPickup GetBloodPickup(Vector3 position, Quaternion rotation)
    {
        return bloodPickupPool.Get(bloodPrefab, position, rotation);
    }

    public void ReleaseBloodPickup(BloodPickup instance)
    {
        bloodPickupPool.Release(instance);
    }

    public PlayerProjectile GetPlayerProjectile(Vector3 position, Quaternion rotation)
    {
        return bulletPool.Get(playerProjectile, position, rotation);
    }

    public void ReleaseBullet(PlayerProjectile instance)
    {
        bulletPool.Release(instance);
    }

    public EnemyProjectile GetEnemyProjectile(Vector3 position, Quaternion rotation)
    {
        return enemyShotPool.Get(enemyProjectile, position, rotation);
    }

    public void ReleaseEnemyProjectile(EnemyProjectile instance)
    {
        enemyShotPool.Release(instance);
    }

    public void Dispose()
    {
        bloodPickupPool.Clear();
        bulletPool.Clear();
        enemyShotPool.Clear();
    }
}
