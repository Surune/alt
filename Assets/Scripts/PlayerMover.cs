using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider playerCollider;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private ShotProjectile shotPrefab;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float shotSpeed = 12f;
    [SerializeField] private float shotRange = 10f;
    [SerializeField] private float shotSpawnDistance = 0.4f;

    private Vector2 moveInput;

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
        Vector3 up = Camera.main.transform.up;
        up.y = 0f;
        up.Normalize();

        Vector3 right = Camera.main.transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 moveDirection = (right * moveInput.x) + (up * moveInput.y);
        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        Vector3 nextPosition = rb.position + (moveDirection * moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(nextPosition);
    }

    private void Shoot()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        float distance = (rb.position.y - ray.origin.y) / ray.direction.y;
        Vector3 targetPosition = ray.GetPoint(distance);
        Vector3 shotDirection = targetPosition - rb.position;
        shotDirection.y = 0f;
        shotDirection.Normalize();

        Vector3 shotStartPosition = rb.position + (shotDirection * shotSpawnDistance);
        ShotProjectile shotInstance = Instantiate(shotPrefab, shotStartPosition, Quaternion.identity);
        shotInstance.Initialize(shotDirection, shotSpeed, shotRange, playerCollider);
    }
}
