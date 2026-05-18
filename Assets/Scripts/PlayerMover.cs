using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PlayerMover : MonoBehaviour
{
    public static event Action<WeaponData> CurrentWeaponChanged;
    public static event Action<int, int> CurrentAmmoChanged;
    public static event Action<bool> ReloadStateChanged;
    public static event Action<int, int> HealthChanged;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider playerCollider;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference fearShotAction;
    [SerializeField] private InputActionReference rollAction;
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int fearShotCount = 1;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rollSpeed = 8f;
    [SerializeField] private float rollDuration = 0.3f;
    [SerializeField] private float damageInvincibilityDuration = 0.5f;

    private readonly List<WeaponData> ownedWeapons = new();
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
    private float damageInvincibilityEndTime;
    private int queuedBurstShots;

    public bool IsRolling => Time.time < rollEndTime;
    public bool IsInvincible => IsRolling || Time.time < damageInvincibilityEndTime;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    private bool IsReloading => Time.time < reloadEndTime;
    private WeaponData CurrentWeapon => ownedWeapons[currentWeaponIndex];
    private int currentWeaponIndex;
    private bool reloadState;

    private void Awake()
    {
        cam = Camera.main;
        currentHealth = maxHealth;
        NotifyHealthChanged();
    }
    
    private void Start()
    {
        var weapons = WeaponCatalog.Instance.Weapons;
        var randomIndex = Random.Range(0, weapons.Length);
        var startWeapon = weapons[randomIndex];

        ownedWeapons.Add(startWeapon);
        currentAmmo = CurrentWeapon.MagazineSize;
        NotifyCurrentWeaponChanged();
        NotifyCurrentAmmoChanged();

        Debug.Log($"Starting weapon: {CurrentWeapon.DisplayName}");
    }

    private void Update()
    {
        if (reloadState && !IsReloading)
        {
            CompleteReload();
        }

        if (!GameStateManager.Instance.IsGameplayActive)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = moveAction.action.ReadValue<Vector2>();
        HandleWeaponScroll();

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
        queuedBurstShots = CurrentWeapon.BurstCount;
        FireBurstShot();
        nextShotTime = Time.time + CurrentWeapon.FireInterval;
    }

    private void FireBurstShot()
    {
        var aimDirection = GetAimDirection();
        FireShotPattern(aimDirection);
        currentAmmo--;
        queuedBurstShots--;
        NotifyCurrentAmmoChanged();

        if (currentAmmo <= 0)
        {
            queuedBurstShots = 0;
            StartReload();
            return;
        }

        if (queuedBurstShots > 0)
        {
            nextBurstShotTime = Time.time + CurrentWeapon.BurstInterval;
        }
    }

    private void StartReload()
    {
        reloadEndTime = Time.time + CurrentWeapon.ReloadDuration;
        currentAmmo = 0;
        nextShotTime = reloadEndTime;
        queuedBurstShots = 0;
        reloadState = true;
        NotifyCurrentAmmoChanged();
        NotifyReloadStateChanged();
        Debug.Log($"Reload started: {CurrentWeapon.DisplayName}");
    }

    private void CompleteReload()
    {
        currentAmmo = CurrentWeapon.MagazineSize;
        reloadState = false;
        NotifyCurrentAmmoChanged();
        NotifyReloadStateChanged();
    }

    private void FireShotPattern(Vector3 aimDirection)
    {
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
        var shotInstance = Instantiate(CurrentWeapon.ProjectilePrefab, rb.position, Quaternion.identity);
        shotInstance.Initialize(
            shotDirection.normalized,
            CurrentWeapon.ProjectileSpeed,
            CurrentWeapon.ProjectileRange,
            CurrentWeapon.Damage
        );
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

        currentAmmo = CurrentWeapon.MagazineSize;
        reloadEndTime = 0f;
        queuedBurstShots = 0;
        nextBurstShotTime = 0f;
        nextShotTime = Time.time;
        reloadState = false;
        NotifyCurrentWeaponChanged();
        NotifyCurrentAmmoChanged();
        NotifyReloadStateChanged();

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

    public void TakeDamage(int damage)
    {
        if (IsInvincible)
        {
            return;
        }

        currentHealth -= damage;
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
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        NotifyHealthChanged();
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
        currentAmmo = CurrentWeapon.MagazineSize;
        reloadEndTime = 0f;
        queuedBurstShots = 0;
        nextBurstShotTime = 0f;
        nextShotTime = Time.time;
        reloadState = false;
        NotifyCurrentWeaponChanged();
        NotifyCurrentAmmoChanged();
        NotifyReloadStateChanged();

        Debug.Log($"Weapon acquired: {CurrentWeapon.DisplayName}");
    }

    private void NotifyCurrentWeaponChanged()
    {
        CurrentWeaponChanged?.Invoke(CurrentWeapon);
    }

    private void NotifyCurrentAmmoChanged()
    {
        CurrentAmmoChanged?.Invoke(currentAmmo, CurrentWeapon.MagazineSize);
    }

    private void NotifyReloadStateChanged()
    {
        ReloadStateChanged?.Invoke(reloadState);
    }

    private void NotifyHealthChanged()
    {
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
