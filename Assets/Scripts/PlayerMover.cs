using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : BillboardObject
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider playerCollider;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private ShotProjectile shotPrefab;
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float shotSpeed = 12f;
    [SerializeField] private float shotRange = 10f;

    private Vector2 moveInput;
    private int currentHealth;
    private int currentExperience;

    protected override void Awake()
    {
        base.Awake();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        moveInput = moveAction.action.ReadValue<Vector2>();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Shoot();
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
        moveDirection.Normalize();

        var nextPosition = rb.position + (moveDirection * moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(nextPosition);
    }

    private void Shoot()
    {
        var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        var distance = (rb.position.y - ray.origin.y) / ray.direction.y;
        var targetPosition = ray.GetPoint(distance);
        var shotDirection = targetPosition - rb.position;
        shotDirection.y = 0f;
        shotDirection.Normalize();

        var shotInstance = Instantiate(shotPrefab, rb.position, Quaternion.identity);
        shotInstance.Initialize(shotDirection, shotSpeed, shotRange);
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
