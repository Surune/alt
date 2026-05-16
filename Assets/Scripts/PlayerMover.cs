using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider playerCollider;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference fearShotAction;
    [SerializeField] private InputActionReference rollAction;
    [SerializeField] private ShotProjectile shotPrefab;
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int fearShotCount = 1;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rollSpeed = 8f;
    [SerializeField] private float rollDuration = 0.3f;
    [SerializeField] private float shotSpeed = 12f;
    [SerializeField] private float shotRange = 10f;
    [SerializeField] private float shotInterval = 0.15f;
    [SerializeField] private int magazineSize = 6;
    [SerializeField] private float reloadDuration = 1.5f;

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

    public bool IsRolling => Time.time < rollEndTime;
    public bool IsInvincible => IsRolling;
    private bool IsReloading => Time.time < reloadEndTime;

    private void Awake()
    {
        cam = Camera.main;
        currentHealth = maxHealth;
        currentAmmo = magazineSize;
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

        if (!IsReloading && Mouse.current.leftButton.isPressed && Time.time >= nextShotTime)
        {
            Shoot();
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

    private void Shoot()
    {
        var shotInstance = Instantiate(shotPrefab, rb.position, Quaternion.identity);
        shotInstance.Initialize(GetAimDirection(), shotSpeed, shotRange);
        currentAmmo--;
        nextShotTime = Time.time + shotInterval;

        if (currentAmmo <= 0)
        {
            StartReload();
        }
    }

    private void StartReload()
    {
        reloadEndTime = Time.time + reloadDuration;
        currentAmmo = magazineSize;
        nextShotTime = reloadEndTime;
        Debug.Log("Reload started");
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
