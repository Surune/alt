using UnityEngine;
using UnityEngine.AI;

public class Enemy : Agent
{
    [SerializeField] protected float awarenessRange = 12f;
    [SerializeField] protected float preferredDistance = 6f;
    [SerializeField] protected float distanceSlack = 1.5f;
    [SerializeField] protected float retreatStep = 3f;
    [SerializeField] protected float strafeDistance = 2f;
    [SerializeField] protected float strafeFrequency = 1.5f;
    [SerializeField] protected float turnSpeed = 720f;
    [SerializeField] protected float repathInterval = 0.2f;
    
    protected Transform player;
    protected float nextRepathTime;

    private void Awake()
    {
        player = FindFirstObjectByType<PlayerMover>().transform;
        currentHealth = maxHealth;
        agent.baseOffset = 0f;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

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
            destination += awayFromPlayer * retreatStep;
            destination += strafeDirection * strafeDistance;
        }
        else if (distance > preferredDistance + distanceSlack)
        {
            destination = player.position;
            destination += awayFromPlayer * preferredDistance;
            destination += strafeDirection * strafeDistance;
        }
        else
        {
            destination += awayFromPlayer * (distanceSlack * 0.5f);
            destination += strafeDirection * strafeDistance;
        }

        var sampledPosition = destination;
        if (NavMesh.SamplePosition(destination, out var hit, 4f, NavMesh.AllAreas))
        {
            sampledPosition = hit.position;
        }

        agent.SetDestination(sampledPosition);
    }
}
