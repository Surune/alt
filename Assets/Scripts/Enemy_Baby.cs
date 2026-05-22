using UnityEngine;
using UnityEngine.AI;

public class Enemy_Baby : Enemy
{
    [SerializeField] protected float awarenessRange = 12f;
    [SerializeField] protected float turnSpeed = 720f;
    [SerializeField] protected float repathInterval = 0.2f;
    
    private float nextRepathTime;

    private void Update()
    {
        if (!GameStateManager.Instance.IsGameplayActive)
        {
            return;
        }

        UpdateFacing();
        UpdateMovement();
    }

    private void UpdateFacing()
    {
        var direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        var targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    private void UpdateMovement()
    {
        if (Time.time < nextRepathTime || !agent.isOnNavMesh)
        {
            return;
        }

        nextRepathTime = Time.time + repathInterval;

        var toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        var distance = toPlayer.magnitude;
        if (distance > awarenessRange)
        {
            agent.ResetPath();
            return;
        }

        if (distance <= 0.001f)
        {
            return;
        }

        var destination = player.position;
        var sampledPosition = destination;
        if (NavMesh.SamplePosition(destination, out var hit, 4f, NavMesh.AllAreas))
        {
            sampledPosition = hit.position;
        }

        agent.SetDestination(sampledPosition);
    }
}
