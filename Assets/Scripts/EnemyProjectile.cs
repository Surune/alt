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
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Player>(out var player))
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        var collidedGameObject = collision.gameObject;
        if (collidedGameObject.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }

    public static void ClearAll()
    {
        for (var i = ActiveProjectiles.Count - 1; i >= 0; i--)
        {
            Destroy(ActiveProjectiles[i].gameObject);
        }
    }
}
