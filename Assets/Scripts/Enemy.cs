using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : BillboardObject
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float awarenessRange = 12f;
    [SerializeField] private float preferredDistance = 6f;
    [SerializeField] private float distanceSlack = 1.5f;
    [SerializeField] private float retreatStep = 3f;
    [SerializeField] private float strafeDistance = 2f;
    [SerializeField] private float strafeFrequency = 1.5f;
    [SerializeField] private float repathInterval = 0.2f;

    private Transform player;
    private float nextRepathTime;
    private int currentHealth;

    protected override void Awake()
    {
        base.Awake();
        player = FindFirstObjectByType<PlayerMover>().transform;
        currentHealth = maxHealth;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
