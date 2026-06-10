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
    private readonly PoolCollection<ShotProjectile> bulletPool = new();
    private readonly PoolCollection<EnemyProjectile> enemyShotPool = new();

    public BloodPickup GetBloodPickup(BloodPickup prefab, Vector3 position, Quaternion rotation)
    {
        return bloodPickupPool.Get(prefab, position, rotation);
    }

    public void ReleaseBloodPickup(BloodPickup instance)
    {
        bloodPickupPool.Release(instance);
    }

    public ShotProjectile GetBullet(ShotProjectile prefab, Vector3 position, Quaternion rotation)
    {
        return bulletPool.Get(prefab, position, rotation);
    }

    public void ReleaseBullet(ShotProjectile instance)
    {
        bulletPool.Release(instance);
    }

    public EnemyProjectile GetEnemyShot(EnemyProjectile prefab, Vector3 position, Quaternion rotation)
    {
        return enemyShotPool.Get(prefab, position, rotation);
    }

    public void ReleaseEnemyShot(EnemyProjectile instance)
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
