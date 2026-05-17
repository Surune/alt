using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider playerCollider;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference fearShotAction;
    [SerializeField] private InputActionReference rollAction;
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int fearShotCount = 1;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rollSpeed = 8f;
    [SerializeField] private float rollDuration = 0.3f;

    private Vector2 moveInput;
    private Vector3 rollDirection;
    private Quaternion rollBaseRotation;
    private float rollStartTime;
    private float rollEndTime;
    private int currentHealth;
    private int currentAmmo;
    private Camera cam;
    private float nextShotTime;
    private float reloadEndTime;
    private float nextBurstShotTime;
    private int queuedBurstShots;

    public bool IsRolling => Time.time < rollEndTime;
    public bool IsInvincible => IsRolling;
    private bool IsReloading => Time.time < reloadEndTime;

    private void Awake()
    {
        cam = Camera.main;
        currentHealth = maxHealth;
        currentAmmo = weaponData.MagazineSize;
    }

    private void Update()
    {
        if (!GameStateManager.Instance.IsGameplayActive)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = moveAction.action.ReadValue<Vector2>();

        if (!IsReloading && currentAmmo <= 0)
        {
            StartReload();
        }

        if (!IsRolling && !IsReloading && queuedBurstShots > 0 && Time.time >= nextBurstShotTime)
        {
            FireBurstShot();
        }

        if (!IsRolling && !IsReloading && queuedBurstShots == 0 && Mouse.current.leftButton.isPressed && Time.time >= nextShotTime)
        {
            StartAttack();
        }

        if (fearShotAction.action.WasPressedThisFrame())
        {
            UseFearShot();
        }
        
        if (rollAction.action.WasPressedThisFrame())
        {
            StartRoll();
        }
    }

    private void FixedUpdate()
    {
        if (!GameStateManager.Instance.IsGameplayActive)
        {
            return;
        }

        var up = cam.transform.up;
        up.y = 0f;
        up.Normalize();

        var right = cam.transform.right;
        right.y = 0f;
        right.Normalize();

        var moveDirection = (right * moveInput.x) + (up * moveInput.y);
        var aimDirection = GetAimDirection();

        if (IsRolling)
        {
            var rollPosition = rb.position + (rollDirection * rollSpeed * Time.fixedDeltaTime);
            var rollProgress = (Time.time - rollStartTime) / rollDuration;
            var rollRotation = rollBaseRotation * Quaternion.Euler(rollProgress * 360f, 0f, 0f);

            rb.MoveRotation(rollRotation);
            rb.MovePosition(rollPosition);
            return;
        }

        if (aimDirection.sqrMagnitude > 0f)
        {
            var targetRotation = Quaternion.LookRotation(aimDirection, Vector3.up);
            rb.MoveRotation(targetRotation);
        }

        moveDirection.Normalize();

        var nextPosition = rb.position + (moveDirection * moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(nextPosition);
    }

    private void StartRoll()
    {
        var moveDirection = GetMoveDirection();
        if (moveDirection.sqrMagnitude > 0f)
        {
            rollDirection = moveDirection;
        }
        else
        {
            rollDirection = transform.forward;
            rollDirection.y = 0f;
            rollDirection.Normalize();
        }

        rollBaseRotation = rb.rotation;
        rollStartTime = Time.time;
        rollEndTime = Time.time + rollDuration;
    }

    private void StartAttack()
    {
        queuedBurstShots = weaponData.BurstCount;
        FireBurstShot();
        nextShotTime = Time.time + weaponData.FireInterval;
    }

    private void FireBurstShot()
    {
        var aimDirection = GetAimDirection();
        FireShotPattern(aimDirection);
        currentAmmo--;
        queuedBurstShots--;

        if (currentAmmo <= 0)
        {
            queuedBurstShots = 0;
            StartReload();
            return;
        }

        if (queuedBurstShots > 0)
        {
            nextBurstShotTime = Time.time + weaponData.BurstInterval;
        }
    }

    private void StartReload()
    {
        reloadEndTime = Time.time + weaponData.ReloadDuration;
        currentAmmo = weaponData.MagazineSize;
        nextShotTime = reloadEndTime;
        queuedBurstShots = 0;
        Debug.Log("Reload started");
    }

    private void FireShotPattern(Vector3 aimDirection)
    {
        if (weaponData.ProjectilesPerShot == 1)
        {
            SpawnProjectile(aimDirection);
            return;
        }

        var halfSpread = weaponData.SpreadAngle * 0.5f;
        var step = weaponData.SpreadAngle / (weaponData.ProjectilesPerShot - 1);

        for (var i = 0; i < weaponData.ProjectilesPerShot; i++)
        {
            var angle = -halfSpread + (step * i);
            var shotDirection = Quaternion.AngleAxis(angle, Vector3.up) * aimDirection;
            SpawnProjectile(shotDirection);
        }
    }

    private void SpawnProjectile(Vector3 shotDirection)
    {
        var shotInstance = Instantiate(weaponData.ProjectilePrefab, rb.position, Quaternion.identity);
        shotInstance.Initialize(
            shotDirection.normalized,
            weaponData.ProjectileSpeed,
            weaponData.ProjectileRange,
            weaponData.Damage
        );
    }

    private void UseFearShot()
    {
        if (fearShotCount <= 0)
        {
            return;
        }

        fearShotCount--;
        EnemyProjectile.ClearAll();
    }

    private Vector3 GetAimDirection()
    {
        var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        var distance = (rb.position.y - ray.origin.y) / ray.direction.y;
        var targetPosition = ray.GetPoint(distance);
        var aimDirection = targetPosition - rb.position;
        aimDirection.y = 0f;
        aimDirection.Normalize();
        return aimDirection;
    }

    private Vector3 GetMoveDirection()
    {
        var up = cam.transform.up;
        up.y = 0f;
        up.Normalize();

        var right = cam.transform.right;
        right.y = 0f;
        right.Normalize();

        var moveDirection = (right * moveInput.x) + (up * moveInput.y);
        moveDirection.Normalize();
        return moveDirection;
    }

    public void TakeDamage(int damage)
    {
        if (IsInvincible)
        {
            return;
        }

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void AddExperience(int amount)
    {
        ExperienceManager.Instance.AddExperience(amount);
    }
}
