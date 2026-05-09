using UnityEngine;

public class ShotProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider shotCollider;

    private Vector3 direction;
    private Vector3 startPosition;
    private float speed;
    private float maxDistance;

    public void Initialize(Vector3 shotDirection, float shotSpeed, float shotRange, Collider ownerCollider)
    {
        direction = shotDirection;
        startPosition = rb.position;
        speed = shotSpeed;
        maxDistance = shotRange;
        Physics.IgnoreCollision(shotCollider, ownerCollider);
        rb.linearVelocity = direction * speed;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = direction * speed;

        Vector3 distanceVector = rb.position - startPosition;
        if (distanceVector.sqrMagnitude >= maxDistance * maxDistance)
        {
            Destroy(gameObject);
        }
    }
}
