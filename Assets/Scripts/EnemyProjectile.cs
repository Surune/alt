using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider shotCollider;
    [SerializeField] private int damage = 1;

    private static readonly List<EnemyProjectile> ActiveProjectiles = new();

    private Vector3 direction;
    private Vector3 startPosition;
    private float speed;
    private float maxDistance;
    private bool isActive;

    private void OnEnable()
    {
        ActiveProjectiles.Add(this);
    }

    private void OnDisable()
    {
        ActiveProjectiles.Remove(this);
    }

    public void Initialize(Vector3 shotDirection, float shotSpeed, float shotRange)
    {
        isActive = true;
        direction = shotDirection;
        startPosition = rb.position;
        speed = shotSpeed;
        maxDistance = shotRange;
        rb.linearVelocity = direction * speed;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = direction * speed;

        var distanceVector = rb.position - startPosition;
        if (distanceVector.sqrMagnitude >= maxDistance * maxDistance)
        {
            Release();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Player>(out var player))
        {
            player.TakeDamage(damage);
            Release();
            return;
        }

        var collidedGameObject = collision.gameObject;
        if (collidedGameObject.CompareTag("Obstacle"))
        {
            Release();
        }
    }

    public static void ClearAll()
    {
        for (var i = ActiveProjectiles.Count - 1; i >= 0; i--)
        {
            ActiveProjectiles[i].Release();
        }
    }

    private void Release()
    {
        if (!isActive)
        {
            return;
        }

        isActive = false;
        rb.linearVelocity = Vector3.zero;
        PoolManager.Instance.ReleaseEnemyShot(this);
    }
}
