using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy_Persuer : Enemy
{
    [SerializeField] private float awarenessRange = 12f;
    [SerializeField] private float turnSpeed = 720f;
    [SerializeField] private float repathInterval = 0.2f;
    [SerializeField] private float timeToReachMaxSpeed = 30f;
    [SerializeField] private float maxSpeedMultiplier = 4f;

    private float pursuitBaseMoveSpeed;
    private float spawnTime;
    private float nextRepathTime;

    private void Start()
    {
        pursuitBaseMoveSpeed = agent.speed;
        spawnTime = Time.time;
    }

    private void Update()
    {
        if (!GameStateManager.Instance.IsGameplayActive)
        {
            return;
        }

        UpdateSpeed();
        UpdateFacing();
        UpdateMovement();
    }

    private void UpdateSpeed()
    {
        var elapsedTime = Time.time - spawnTime;
        var progress = Mathf.Clamp01(elapsedTime / timeToReachMaxSpeed);
        var speedMultiplier = Mathf.Lerp(1f, maxSpeedMultiplier, progress);
        agent.speed = pursuitBaseMoveSpeed * speedMultiplier;
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
