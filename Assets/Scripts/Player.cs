using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public static event Action<WeaponData> CurrentWeaponChanged;
    public static event Action<int> MagazineAmmoChanged;
    public static event Action<int> HealthChanged;
    public static event Action Damaged;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference fearShotAction;
    [SerializeField] private WeaponData startingWeapon;
    [SerializeField] private int startingHealth = 25;
    [SerializeField] private int fearShotCount = 1;
    [SerializeField] private float speed = 4.5f;
    [SerializeField] private float damageInvincibilityDuration = 0.5f;

    private readonly List<WeaponData> ownedWeapons = new();
    private readonly Dictionary<WeaponData, int> magazineAmmo = new();
    private Vector2 moveInput;
    private int currentHealth;
    private Camera cam;
    private float nextShotTime;
    private float nextBurstShotTime;
    private float reloadEndTime;
    private float damageInvincibilityEndTime;
    private int queuedBurstShots;
    private int barrier;
    private Vector3 lastPosition;
    private float movedDistance;
    private bool isReloading;

    public int CurrentHealth => currentHealth;
    
    private WeaponData CurrentWeapon => ownedWeapons[currentWeaponIndex];
    private bool IsInvincible => Time.time < damageInvincibilityEndTime;
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
        ownedWeapons.Add(startingWeapon);
        magazineAmmo.Add(startingWeapon, startingWeapon.MagazineSize);
        NotifyCurrentWeaponChanged();

        Debug.Log($"Starting weapon: {CurrentWeapon.DisplayName}");
    }

    private void Update()
    {
        if (!GameManager.Instance.GameState.IsGameplayActive)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = moveAction.action.ReadValue<Vector2>();
        HandleWeaponScroll();

        if (isReloading && Time.time >= reloadEndTime)
        {
            CompleteReload();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            StartReload();
        }

        if (queuedBurstShots > 0 && Time.time >= nextBurstShotTime)
        {
            FireBurstShot();
        }

        if (queuedBurstShots == 0 && Mouse.current.leftButton.isPressed && Time.time >= nextShotTime)
        {
            StartAttack();
        }

        if (fearShotAction.action.WasPressedThisFrame())
        {
            UseFearShot();
        }
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.GameState.IsGameplayActive)
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

    private void StartAttack()
    {
        if (magazineAmmo[CurrentWeapon] == 0)
        {
            StartReload();
            return;
        }

        if (!CanFireCurrentWeapon())
        {
            return;
        }

        queuedBurstShots = CurrentWeapon.BurstCount;
        FireBurstShot();
        nextShotTime = Time.time + GameManager.Instance.Ability.GetFireInterval(CurrentWeapon.FireInterval);
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
        magazineAmmo[CurrentWeapon]--;
        NotifyMagazineAmmoChanged();
        currentHealth -= CurrentWeapon.ProjectilesPerShot;
        queuedBurstShots--;
        NotifyHealthChanged();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (magazineAmmo[CurrentWeapon] == 0)
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

    private bool CanFireCurrentWeapon()
    {
        return !isReloading
            && magazineAmmo[CurrentWeapon] > 0
            && GameManager.Instance.Ability.CanFire
            && currentHealth > CurrentWeapon.ProjectilesPerShot;
    }

    private void StartReload()
    {
        if (isReloading || magazineAmmo[CurrentWeapon] == CurrentWeapon.MagazineSize)
        {
            return;
        }

        isReloading = true;
        queuedBurstShots = 0;
        reloadEndTime = Time.time + CurrentWeapon.ReloadDuration;
    }

    private void CompleteReload()
    {
        magazineAmmo[CurrentWeapon] = CurrentWeapon.MagazineSize;
        isReloading = false;
        nextShotTime = Time.time;
        NotifyMagazineAmmoChanged();
    }

    private void FireShotPattern(Vector3 aimDirection)
    {
        if (GameManager.Instance.Ability.Fracture)
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
        FireProjectile(shotDirection, CurrentWeapon.Damage, 0, false, true);
        if (GameManager.Instance.Ability.Awaken)
        {
            FireProjectile(Quaternion.AngleAxis(-8f, Vector3.up) * shotDirection, CurrentWeapon.Damage, 0, false, false);
            FireProjectile(Quaternion.AngleAxis(8f, Vector3.up) * shotDirection, CurrentWeapon.Damage, 0, false, false);
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
        isReloading = false;
        NotifyCurrentWeaponChanged();
        NotifyMagazineAmmoChanged();

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

    public void TakeDamage(float damage)
    {
        if (IsInvincible)
        {
            return;
        }

        if (barrier > 0)
        {
            barrier--;
            GameManager.Instance.Ability.OnBarrierBroken();
            return;
        }

        currentHealth -= Mathf.CeilToInt(damage);
        GameManager.Instance.Ability.OnPlayerDamaged(damage);
        damageInvincibilityEndTime = Time.time + damageInvincibilityDuration;
        Damaged?.Invoke();
        NotifyHealthChanged();
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        queuedBurstShots = 0;
        SceneManager.LoadScene("GameOverScene");
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

    public Vector3 GetNearestEnemyDirection(Vector3 origin)
    {
        var direction = Enemy.GetNearestPosition(origin) - origin;
        direction.y = 0f;
        return direction.normalized;
    }

    public void FireAbilityProjectile(Vector3 direction, float projectileDamage, int pierce, bool homing)
    {
        FireProjectile(direction, projectileDamage, pierce, homing, false);
    }

    public void FireWingProjectile(Vector3 origin, Vector3 direction, float projectileDamage, float speedCoefficient, bool homing, bool freezing)
    {
        var shotInstance = GameManager.Instance.Pool.GetPlayerProjectile(origin, Quaternion.identity);
        var abilityData = GameManager.Instance.Ability.CreateWingProjectileData(projectileDamage, homing, freezing);
        shotInstance.Initialize(
            direction.normalized,
            GameManager.Instance.Ability.GetProjectileSpeed(CurrentWeapon.ProjectileSpeed) * speedCoefficient,
            CurrentWeapon.ProjectileRange,
            abilityData,
            false
        );
        shotInstance.gameObject.SetActive(true);
    }

    public void FireFatalProjectile(Vector3 direction)
    {
        var shotInstance = GameManager.Instance.Pool.GetPlayerProjectile(rb.position, Quaternion.identity);
        shotInstance.Initialize(direction.normalized, GameManager.Instance.Ability.GetProjectileSpeed(CurrentWeapon.ProjectileSpeed), CurrentWeapon.ProjectileRange, GameManager.Instance.Ability.CreateProjectileData(0f), false);
        shotInstance.ForceFatal();
        shotInstance.gameObject.SetActive(true);
    }

    public void FireCriticalProjectile(Vector3 direction)
    {
        var shotInstance = GameManager.Instance.Pool.GetPlayerProjectile(rb.position, Quaternion.identity);
        shotInstance.Initialize(direction.normalized, GameManager.Instance.Ability.GetProjectileSpeed(CurrentWeapon.ProjectileSpeed), CurrentWeapon.ProjectileRange, GameManager.Instance.Ability.CreateProjectileData(CurrentWeapon.Damage), false);
        shotInstance.ForceCritical();
        shotInstance.gameObject.SetActive(true);
    }

    private void FireProjectile(Vector3 direction, float projectileDamage, int pierce, bool homing, bool dropsBloodPickup)
    {
        var shotInstance = GameManager.Instance.Pool.GetPlayerProjectile(rb.position, Quaternion.identity);
        var abilityData = GameManager.Instance.Ability.CreateProjectileData(projectileDamage, pierce, homing);
        shotInstance.Initialize(
            direction.normalized,
            GameManager.Instance.Ability.GetProjectileSpeed(CurrentWeapon.ProjectileSpeed),
            CurrentWeapon.ProjectileRange,
            abilityData,
            dropsBloodPickup
        );
        shotInstance.gameObject.SetActive(true);
    }

    private void NotifyCurrentWeaponChanged()
    {
        CurrentWeaponChanged?.Invoke(CurrentWeapon);
    }

    private void NotifyMagazineAmmoChanged()
    {
        MagazineAmmoChanged?.Invoke(magazineAmmo[CurrentWeapon]);
    }

    private void NotifyHealthChanged()
    {
        HealthChanged?.Invoke(currentHealth);
    }
}
