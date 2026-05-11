using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    [SerializeField] private Transform mapPlane;
    [SerializeField] private EnemyProjectile shotPrefab;
    [SerializeField] private float spawnInterval = 0.4f;
    [SerializeField] private float shotSpeed = 5f;
    [SerializeField] private float shotRange = 16f;
    [SerializeField] private float edgePadding = 0.35f;
    [SerializeField] private float directionJitter = 65f;

    private float nextSpawnTime;

    private void Update()
    {
        if (Time.time < nextSpawnTime)
        {
            return;
        }

        nextSpawnTime = Time.time + spawnInterval;
        SpawnShot();
    }

    private void SpawnShot()
    {
        var planePosition = mapPlane.position;
        var halfWidth = (mapPlane.lossyScale.x * 5f) - edgePadding;
        var halfHeight = (mapPlane.lossyScale.z * 5f) - edgePadding;
        var side = Random.Range(0, 4);
        var spawnPosition = planePosition;
        var inwardDirection = Vector3.forward;

        switch (side)
        {
            case 0:
                spawnPosition.x -= halfWidth;
                spawnPosition.z += Random.Range(-halfHeight, halfHeight);
                inwardDirection = Vector3.right;
                break;
            case 1:
                spawnPosition.x += halfWidth;
                spawnPosition.z += Random.Range(-halfHeight, halfHeight);
                inwardDirection = Vector3.left;
                break;
            case 2:
                spawnPosition.z += halfHeight;
                spawnPosition.x += Random.Range(-halfWidth, halfWidth);
                inwardDirection = Vector3.back;
                break;
            default:
                spawnPosition.z -= halfHeight;
                spawnPosition.x += Random.Range(-halfWidth, halfWidth);
                inwardDirection = Vector3.forward;
                break;
        }

        var shotDirection = Quaternion.AngleAxis(Random.Range(-directionJitter, directionJitter), Vector3.up) * inwardDirection;
        var shotInstance = Instantiate(shotPrefab, spawnPosition, Quaternion.identity);
        shotInstance.Initialize(shotDirection, shotSpeed, shotRange);
    }
}
