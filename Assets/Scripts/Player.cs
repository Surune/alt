using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    public static event Action<WeaponData> CurrentWeaponChanged;
    public static event Action<int> HealthChanged;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider playerCollider;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference fearShotAction;
    [SerializeField] private InputActionReference rollAction;
    [SerializeField] private int startingHealth = 25;
    [SerializeField] private int fearShotCount = 1;
    [SerializeField] private float speed = 4.5f;
    [SerializeField] private float rollDuration = 0.3f;
    [SerializeField] private float rollCooldown = 0.5f;
    [SerializeField] private float damageInvincibilityDuration = 0.5f;

    private readonly List<WeaponData> ownedWeapons = new();
    private Vector2 moveInput;
    private Vector3 rollDirection;
    private Quaternion rollBaseRotation;
    private float rollStartTime;
    private float rollEndTime;
    private float nextRollTime;
    private int currentHealth;
    private Camera cam;
    private float nextShotTime;
    private float nextBurstShotTime;
    private float damageInvincibilityEndTime;
    private int queuedBurstShots;
    private int barrier;
    private Vector3 lastPosition;
    private float movedDistance;

    public int CurrentHealth => currentHealth;
    
    private WeaponData CurrentWeapon => ownedWeapons[currentWeaponIndex];
    private bool IsRolling => Time.time < rollEndTime;
    private bool IsInvincible => IsRolling || Time.time < damageInvincibilityEndTime;
    private int currentWeaponIndex;

    private void Awake()
    {
        cam = Camera.main;
        currentHealth = startingHealth;
        lastPosition = rb.position;
        NotifyHealthChanged();
    }
    
    private void Start()
    {
        var weapons = WeaponCatalog.Instance.Weapons;
        var randomIndex = Random.Range(0, weapons.Length);
        var startWeapon = weapons[randomIndex];

        ownedWeapons.Add(startWeapon);
        NotifyCurrentWeaponChanged();

        Debug.Log($"Starting weapon: {CurrentWeapon.DisplayName}");
    }

    private void Update()
    {
        if (!GameStateManager.Instance.IsGameplayActive)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = moveAction.action.ReadValue<Vector2>();
        HandleWeaponScroll();

        if (!IsRolling && queuedBurstShots > 0 && Time.time >= nextBurstShotTime)
        {
            FireBurstShot();
        }

        if (!IsRolling && queuedBurstShots == 0 && Mouse.current.leftButton.isPressed && Time.time >= nextShotTime)
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
            var rollPosition = rb.position + (rollDirection * speed * Time.fixedDeltaTime);
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

        var nextPosition = rb.position + (moveDirection * speed * Time.fixedDeltaTime);
        rb.MovePosition(nextPosition);
        movedDistance += Vector3.Distance(lastPosition, nextPosition);
        lastPosition = nextPosition;
    }

    private void StartRoll()
    {
        if (Time.time < nextRollTime)
        {
            return;
        }

        var moveDirection = GetMoveDirection();
        if (moveDirection.sqrMagnitude > 0f)
        {
            rollDirection = moveDirection;
        }
        else
        {
            rollDirection = Vector3.zero;
        }

        rollBaseRotation = rb.rotation;
        rollStartTime = Time.time;
        rollEndTime = Time.time + rollDuration;
        nextRollTime = Time.time + rollCooldown;
    }

    private void StartAttack()
    {
        if (!CanFireCurrentWeapon())
        {
            return;
        }

        queuedBurstShots = CurrentWeapon.BurstCount;
        FireBurstShot();
        nextShotTime = Time.time + AbilityManager.Instance.GetFireInterval(CurrentWeapon.FireInterval);
    }

    private void FireBurstShot()
    {
        if (!CanFireCurrentWeapon())
        {
            queuedBurstShots = 0;
            return;
        }

        var aimDirection = GetAimDirection();
        FireShotPattern(aimDirection);
        currentHealth -= CurrentWeapon.ProjectilesPerShot;
        queuedBurstShots--;
        NotifyHealthChanged();

        if (currentHealth <= 0)
        {
            queuedBurstShots = 0;
            Destroy(gameObject);
            return;
        }

        if (queuedBurstShots > 0)
        {
            nextBurstShotTime = Time.time + CurrentWeapon.BurstInterval;
        }
    }

    private bool CanFireCurrentWeapon()
    {
        return AbilityManager.Instance.CanFire && currentHealth > CurrentWeapon.ProjectilesPerShot;
    }

    private void FireShotPattern(Vector3 aimDirection)
    {
        if (AbilityManager.Instance.Fracture)
        {
            SpawnProjectile(Quaternion.AngleAxis(-45f, Vector3.up) * aimDirection);
            SpawnProjectile(Quaternion.AngleAxis(45f, Vector3.up) * aimDirection);
            return;
        }

        if (CurrentWeapon.ProjectilesPerShot == 1)
        {
            SpawnProjectile(aimDirection);
            return;
        }

        var halfSpread = CurrentWeapon.SpreadAngle * 0.5f;
        var step = CurrentWeapon.SpreadAngle / (CurrentWeapon.ProjectilesPerShot - 1);

        for (var i = 0; i < CurrentWeapon.ProjectilesPerShot; i++)
        {
            var angle = -halfSpread + (step * i);
            var shotDirection = Quaternion.AngleAxis(angle, Vector3.up) * aimDirection;
            SpawnProjectile(shotDirection);
        }
    }

    private void SpawnProjectile(Vector3 shotDirection)
    {
        FireProjectile(shotDirection, CurrentWeapon.Damage, 0, false);
        if (AbilityManager.Instance.Awaken)
        {
            FireProjectile(Quaternion.AngleAxis(-8f, Vector3.up) * shotDirection, CurrentWeapon.Damage, 0, false);
            FireProjectile(Quaternion.AngleAxis(8f, Vector3.up) * shotDirection, CurrentWeapon.Damage, 0, false);
        }
    }

    private void HandleWeaponScroll()
    {
        if (ownedWeapons.Count < 2)
        {
            return;
        }

        var scrollValue = Mouse.current.scroll.ReadValue().y;
        if (scrollValue > 0f)
        {
            SwitchWeapon(1);
            return;
        }

        if (scrollValue < 0f)
        {
            SwitchWeapon(-1);
        }
    }

    private void SwitchWeapon(int direction)
    {
        currentWeaponIndex += direction;
        if (currentWeaponIndex >= ownedWeapons.Count)
        {
            currentWeaponIndex = 0;
        }
        else if (currentWeaponIndex < 0)
        {
            currentWeaponIndex = ownedWeapons.Count - 1;
        }

        queuedBurstShots = 0;
        nextBurstShotTime = 0f;
        nextShotTime = Time.time;
        NotifyCurrentWeaponChanged();

        Debug.Log($"Switched weapon: {CurrentWeapon.DisplayName}");
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

    public void TakeDamage(float damage)
    {
        if (IsInvincible)
        {
            return;
        }

        if (barrier > 0)
        {
            barrier--;
            AbilityManager.Instance.OnBarrierBroken();
            return;
        }

        currentHealth -= Mathf.CeilToInt(damage);
        AbilityManager.Instance.OnPlayerDamaged(damage);
        damageInvincibilityEndTime = Time.time + damageInvincibilityDuration;
        NotifyHealthChanged();
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        NotifyHealthChanged();
    }

    public void AddBarrier(int amount)
    {
        barrier += amount;
    }

    public bool CanSpendHealth(int amount)
    {
        return currentHealth > amount;
    }

    public void SpendHealth(int amount)
    {
        currentHealth -= amount;
        NotifyHealthChanged();
    }

    public void ReduceHealthByPercent(float percent)
    {
        currentHealth -= Mathf.RoundToInt(currentHealth * percent);
        NotifyHealthChanged();
    }

    public void ScaleHealth(float scale)
    {
        currentHealth = Mathf.CeilToInt(currentHealth * scale);
        NotifyHealthChanged();
    }

    public float ConsumeMovedDistance()
    {
        var distance = movedDistance;
        movedDistance = 0f;
        return distance;
    }

    public Vector3 GetDirectionTo(Vector3 position)
    {
        var direction = position - rb.position;
        direction.y = 0f;
        return direction.normalized;
    }

    public Vector3 GetNearestEnemyDirection()
    {
        return GetDirectionTo(Enemy.GetNearestPosition(rb.position));
    }

    public void FireAbilityProjectile(Vector3 direction, float projectileDamage, int pierce, bool homing)
    {
        FireProjectile(direction, projectileDamage, pierce, homing);
    }

    public void FireWingProjectile(Vector3 direction, float projectileDamage, float speedCoefficient, bool homing, bool freezing)
    {
        var shotInstance = PoolManager.Instance.GetBullet(CurrentWeapon.ProjectilePrefab, rb.position, Quaternion.identity);
        var abilityData = AbilityManager.Instance.CreateWingProjectileData(projectileDamage, homing, freezing);
        shotInstance.Initialize(
            direction.normalized,
            AbilityManager.Instance.GetProjectileSpeed(CurrentWeapon.ProjectileSpeed) * speedCoefficient,
            CurrentWeapon.ProjectileRange,
            abilityData
        );
        shotInstance.gameObject.SetActive(true);
    }

    public void FireFatalProjectile(Vector3 direction)
    {
        var shotInstance = PoolManager.Instance.GetBullet(CurrentWeapon.ProjectilePrefab, rb.position, Quaternion.identity);
        shotInstance.Initialize(direction.normalized, AbilityManager.Instance.GetProjectileSpeed(CurrentWeapon.ProjectileSpeed), CurrentWeapon.ProjectileRange, AbilityManager.Instance.CreateProjectileData(0f));
        shotInstance.ForceFatal();
        shotInstance.gameObject.SetActive(true);
    }

    public void FireCriticalProjectile(Vector3 direction)
    {
        var shotInstance = PoolManager.Instance.GetBullet(CurrentWeapon.ProjectilePrefab, rb.position, Quaternion.identity);
        shotInstance.Initialize(direction.normalized, AbilityManager.Instance.GetProjectileSpeed(CurrentWeapon.ProjectileSpeed), CurrentWeapon.ProjectileRange, AbilityManager.Instance.CreateProjectileData(CurrentWeapon.Damage));
        shotInstance.ForceCritical();
        shotInstance.gameObject.SetActive(true);
    }

    private void FireProjectile(Vector3 direction, float projectileDamage, int pierce, bool homing)
    {
        var shotInstance = PoolManager.Instance.GetBullet(CurrentWeapon.ProjectilePrefab, rb.position, Quaternion.identity);
        var abilityData = AbilityManager.Instance.CreateProjectileData(projectileDamage, pierce, homing);
        shotInstance.Initialize(
            direction.normalized,
            AbilityManager.Instance.GetProjectileSpeed(CurrentWeapon.ProjectileSpeed),
            CurrentWeapon.ProjectileRange,
            abilityData
        );
        shotInstance.gameObject.SetActive(true);
    }

    public WeaponData[] GetUnownedWeapons()
    {
        var catalogWeapons = WeaponCatalog.Instance.Weapons;
        var unownedWeapons = new List<WeaponData>();

        for (var i = 0; i < catalogWeapons.Length; i++)
        {
            var weapon = catalogWeapons[i];
            if (!ownedWeapons.Contains(weapon))
            {
                unownedWeapons.Add(weapon);
            }
        }

        return unownedWeapons.ToArray();
    }

    public void AcquireWeapon(WeaponData newWeapon)
    {
        ownedWeapons.Add(newWeapon);
        currentWeaponIndex = ownedWeapons.Count - 1;
        queuedBurstShots = 0;
        nextBurstShotTime = 0f;
        nextShotTime = Time.time;
        NotifyCurrentWeaponChanged();

        Debug.Log($"Weapon acquired: {CurrentWeapon.DisplayName}");
    }

    private void NotifyCurrentWeaponChanged()
    {
        CurrentWeaponChanged?.Invoke(CurrentWeapon);
    }

    private void NotifyHealthChanged()
    {
        HealthChanged?.Invoke(currentHealth);
    }
}
