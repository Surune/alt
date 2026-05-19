using UnityEngine;
using Random = UnityEngine.Random;

public class ShotProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider shotCollider;
    [SerializeField] private int damage = 1;
    [SerializeField] private BloodPickup bloodPickupPrefab;

    private Vector3 direction;
    private Vector3 startPosition;
    private float speed;
    private float maxDistance;

    public void Initialize(Vector3 shotDirection, float shotSpeed, float shotRange, int shotDamage)
    {
        direction = shotDirection;
        startPosition = rb.position;
        speed = shotSpeed;
        maxDistance = shotRange;
        damage = shotDamage;
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
        var enemy = collision.gameObject.GetComponent<Agent>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        var collidedGameObject = collision.gameObject;
        if (collidedGameObject.CompareTag("Obstacle"))
        {
            SpawnBloodPickup();
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
