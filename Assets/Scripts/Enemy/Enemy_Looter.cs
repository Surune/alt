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
        if (!GameManager.Instance.GameState.IsGameplayActive)
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

        var strafeAngle = Mathf.Atan2(strafeDistance, escapeStep) * Mathf.Rad2Deg;
        if (Mathf.Sin(Time.time * strafeFrequency) < 0f)
        {
            strafeAngle = -strafeAngle;
        }

        var distanceGap = Mathf.Max(0f, safeDistance - distance);
        var escapeDistance = escapeStep + distanceGap;
        var origin = agent.nextPosition;
        var destination = origin;
        var bestDistanceFromPlayer = float.MinValue;
        var minimumTravelSqr = escapeStep * escapeStep * 0.25f;
        var preferredDirection = Quaternion.AngleAxis(strafeAngle, Vector3.up) * awayFromPlayer;

        for (var angleIndex = -6; angleIndex <= 6; angleIndex++)
        {
            var direction = Quaternion.AngleAxis(angleIndex * 30f, Vector3.up) * preferredDirection;
            EvaluateDestination(direction * escapeDistance, origin, minimumTravelSqr,
                ref destination, ref bestDistanceFromPlayer);
        }

        agent.SetDestination(destination);
    }

    private void EvaluateDestination(Vector3 offset, Vector3 origin, float minimumTravelSqr,
        ref Vector3 destination, ref float bestDistanceFromPlayer)
    {
        var candidate = origin + offset;
        if (NavMesh.Raycast(origin, candidate, out var hit, NavMesh.AllAreas))
        {
            candidate = hit.position;
        }

        if ((candidate - origin).sqrMagnitude < minimumTravelSqr)
        {
            return;
        }

        var distanceFromPlayer = (candidate - player.position).sqrMagnitude;
        if (distanceFromPlayer <= bestDistanceFromPlayer)
        {
            return;
        }

        bestDistanceFromPlayer = distanceFromPlayer;
        destination = candidate;
    }
}
