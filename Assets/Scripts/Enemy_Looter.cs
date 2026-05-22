using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy_Looter : Enemy
{
    [SerializeField] private float safeDistance = 16f;
    [SerializeField] private float escapeStep = 7.5f;
    [SerializeField] private float strafeDistance = 2.5f;
    [SerializeField] private float repathInterval = 0.2f;
    [SerializeField] private float turnSpeed = 720f;
    [SerializeField] private float strafeFrequency = 1.1f;

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
        var direction = transform.position - player.position;
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

        var awayFromPlayer = transform.position - player.position;
        awayFromPlayer.y = 0f;

        var distance = awayFromPlayer.magnitude;
        if (distance <= 0.001f)
        {
            awayFromPlayer = transform.forward;
            distance = 1f;
        }

        awayFromPlayer /= distance;

        var strafeDirection = Vector3.Cross(Vector3.up, awayFromPlayer);
        if (Mathf.Sin(Time.time * strafeFrequency) < 0f)
        {
            strafeDirection = -strafeDirection;
        }

        var distanceGap = Mathf.Max(0f, safeDistance - distance);
        var escapeDistance = escapeStep + distanceGap;

        var destination = transform.position;
        destination += awayFromPlayer * escapeDistance;
        destination += strafeDirection * strafeDistance;

        if (NavMesh.SamplePosition(destination, out var hit, 5f, NavMesh.AllAreas))
        {
            destination = hit.position;
        }

        agent.SetDestination(destination);
    }
}
