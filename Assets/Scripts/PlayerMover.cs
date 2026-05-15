using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider playerCollider;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference fearShotAction;
    [SerializeField] private ShotProjectile shotPrefab;
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int fearShotCount = 1;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float shotSpeed = 12f;
    [SerializeField] private float shotRange = 10f;

    private Vector2 moveInput;
    private int currentHealth;
    private int currentExperience;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
        currentHealth = maxHealth;
    }

    private void Update()
    {
        moveInput = moveAction.action.ReadValue<Vector2>();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Shoot();
        }

        if (fearShotAction.action.WasPressedThisFrame())
        {
            UseFearShot();
        }
    }

    private void FixedUpdate()
    {
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

        var nextPosition = rb.position + (moveDirection * moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(nextPosition);
    }

    private void Shoot()
    {
        var shotInstance = Instantiate(shotPrefab, rb.position, Quaternion.identity);
        shotInstance.Initialize(GetAimDirection(), shotSpeed, shotRange);
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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void AddExperience(int amount)
    {
        currentExperience += amount;
    }
}
