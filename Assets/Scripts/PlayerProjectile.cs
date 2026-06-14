using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider shotCollider;
    [SerializeField] private float damage = 1;
    [SerializeField] private BloodPickup bloodPickupPrefab;

    private Vector3 direction;
    private Vector3 startPosition;
    private float speed;
    private float maxDistance;
    private ProjectileAbilityData abilityData;
    private int remainingPierce;
    private bool isActive;
    private bool dropsBloodPickup;

    public void Initialize(Vector3 shotDirection, float shotSpeed, float shotRange, ProjectileAbilityData projectileAbilityData, bool dropsBloodPickup)
    {
        isActive = true;
        direction = shotDirection;
        startPosition = rb.position;
        speed = shotSpeed;
        maxDistance = shotRange;
        abilityData = projectileAbilityData;
        damage = abilityData.Damage;
        remainingPierce = abilityData.Pierce;
        this.dropsBloodPickup = dropsBloodPickup;
        rb.linearVelocity = direction * speed;
    }

    private void FixedUpdate()
    {
        if (abilityData.Homing)
        {
            direction = Vector3.RotateTowards(direction, Enemy.GetNearestPosition(rb.position) - rb.position, Time.fixedDeltaTime * 4f, 0f).normalized;
        }

        rb.linearVelocity = direction * speed;

        var distanceVector = rb.position - startPosition;
        if (distanceVector.sqrMagnitude >= maxDistance * maxDistance)
        {
            Release();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        var enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            if (abilityData.IsFatal)
            {
                enemy.TakeFatalDamage(GameManager.Instance.Ability.BossFatalDamage);
            }
            else
            {
                var hitDamage = abilityData.Culling && enemy.IsFullHealth ? damage * 1.25f : damage;
                enemy.TakeDamage(hitDamage);
            }

            if (abilityData.AllTarget)
            {
                Enemy.DamageAll(damage, enemy);
            }

            if (abilityData.Freezing)
            {
                enemy.Slow(0.75f, 2f);
            }

            GameManager.Instance.Ability.OnProjectileHit(enemy, abilityData);
            if (dropsBloodPickup)
            {
                SpawnBloodPickup();
            }

            if (remainingPierce > 0)
            {
                remainingPierce--;
                Physics.IgnoreCollision(shotCollider, collision.collider);
                return;
            }

            Release();
            return;
        }

        var collidedGameObject = collision.gameObject;
        if (collidedGameObject.CompareTag("Obstacle"))
        {
            Release();
        }
    }

    public void ForceFatal()
    {
        abilityData = new ProjectileAbilityData(damage, abilityData.IsCritical, true, abilityData.Pierce, abilityData.Homing, abilityData.AllTarget, abilityData.Freezing, abilityData.Culling, abilityData.Psychosense);
    }

    public void ForceCritical()
    {
        abilityData = new ProjectileAbilityData(damage * 1.5f, true, abilityData.IsFatal, abilityData.Pierce, abilityData.Homing, abilityData.AllTarget, abilityData.Freezing, abilityData.Culling, abilityData.Psychosense);
        damage = abilityData.Damage;
    }

    private void SpawnBloodPickup()
    {
        var bloodPosition = transform.position;
        bloodPosition.y = 0.02f;
        var bloodRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        BloodPickup.SpawnOrGrow(bloodPosition, bloodRotation, 1);
    }

    private void Release()
    {
        if (!isActive)
        {
            return;
        }

        isActive = false;
        rb.linearVelocity = Vector3.zero;
        GameManager.Instance.Pool.ReleaseBullet(this);
    }
}
