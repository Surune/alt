using UnityEngine;

public class ShotProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider shotCollider;
    [SerializeField] private int damage = 1;

    private Vector3 direction;
    private Vector3 startPosition;
    private float speed;
    private float maxDistance;

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
        var enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        var collidedGameObject = collision.gameObject;
        if (collidedGameObject.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
