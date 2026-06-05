using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy_Junkie : Enemy
{
    [SerializeField] private EnemyProjectile shotPrefab;
    [SerializeField] private float awarenessRange = 13f;
    [SerializeField] private float preferredDistance = 6f;
    [SerializeField] private float distanceSlack = 1f;
    [SerializeField] private float retreatDistance = 3.5f;
    [SerializeField] private float approachOffset = 1f;
    [SerializeField] private float strafeDistance = 1.25f;
    [SerializeField] private float strafeFrequency = 1.6f;
    [SerializeField] private float repathInterval = 0.2f;
    [SerializeField] private float turnSpeed = 720f;
    [SerializeField] private float fireInterval = 1.1f;
    [SerializeField] private float shotSpeed = 8f;
    [SerializeField] private float shotRange = 16f;

    private float nextRepathTime;
    private float nextShotTime;

    private void Update()
    {
        if (!GameStateManager.Instance.IsGameplayActive)
        {
            return;
        }

        UpdateFacing();
        UpdateMovement();
        UpdateAttack();
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

        toPlayer /= distance;

        var awayFromPlayer = -toPlayer;
        var strafeDirection = Vector3.Cross(Vector3.up, toPlayer);
        if (Mathf.Sin(Time.time * strafeFrequency) < 0f)
        {
            strafeDirection = -strafeDirection;
        }

        var destination = transform.position;

        if (distance < preferredDistance)
        {
            destination += awayFromPlayer * retreatDistance;
            destination += strafeDirection * strafeDistance;
        }
        else if (distance > preferredDistance + distanceSlack)
        {
            destination = player.position;
            destination += awayFromPlayer * approachOffset;
            destination += strafeDirection * (strafeDistance * 0.5f);
        }
        else
        {
            destination += strafeDirection * strafeDistance;
        }

        var sampledPosition = destination;
        if (NavMesh.SamplePosition(destination, out var hit, 4f, NavMesh.AllAreas))
        {
            sampledPosition = hit.position;
        }

        agent.SetDestination(sampledPosition);
    }

    private void UpdateAttack()
    {
        if (Time.time < nextShotTime)
        {
            return;
        }

        var toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        var distance = toPlayer.magnitude;
        if (distance > awarenessRange)
        {
            return;
        }

        if (distance <= 0.001f)
        {
            return;
        }

        nextShotTime = Time.time + fireInterval;
        SpawnProjectile(toPlayer.normalized);
    }

    private void SpawnProjectile(Vector3 shotDirection)
    {
        var shot = Instantiate(shotPrefab, transform.position, Quaternion.identity);
        shot.Initialize(shotDirection.normalized, shotSpeed, shotRange);
    }
}
