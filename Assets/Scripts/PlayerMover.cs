using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private float moveSpeed = 5f;

    private Vector2 moveInput;

    private void Update()
    {
        moveInput = moveAction.action.ReadValue<Vector2>();
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
}
