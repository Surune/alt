using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider shotCollider;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float range = 15f;
    
    private static readonly List<EnemyProjectile> ActiveProjectiles = new();

    private Vector3 direction;
    private Vector3 startPosition;
    private int damage;
    private bool isActive;

    private void OnEnable()
    {
        ActiveProjectiles.Add(this);
    }

    private void OnDisable()
    {
        ActiveProjectiles.Remove(this);
    }

    public void Initialize(Vector3 shotDirection)
    {
        isActive = true;
        direction = shotDirection;
        startPosition = rb.position;
        rb.linearVelocity = direction * speed;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = direction * speed;

        var distanceVector = rb.position - startPosition;
        if (distanceVector.sqrMagnitude >= range * range)
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

    private void Release()
    {
        if (!isActive)
        {
            return;
        }

        isActive = false;
        rb.linearVelocity = Vector3.zero;
        GameManager.Instance.Pool.ReleaseEnemyProjectile(this);
    }
}
