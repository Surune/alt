using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy_Fly : Enemy
{
    [SerializeField] private EnemyProjectile shotPrefab;
    [SerializeField] private float awarenessRange = 14f;
    [SerializeField] private float turnSpeed = 720f;
    [SerializeField] private float repathInterval = 0.2f;
    [SerializeField] private float shotSpeed = 7f;
    [SerializeField] private float shotRange = 18f;

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
        if (NavMesh.SamplePosition(destination, out var hit, 4f, NavMesh.AllAreas))
        {
            destination = hit.position;
        }

        agent.SetDestination(destination);
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        if (currentHealth <= 0f)
        {
            return;
        }

        FireRandomProjectile();
    }

    private void FireRandomProjectile()
    {
        var angle = Random.Range(0f, 360f);
        var shotDirection = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
        var shot = Instantiate(shotPrefab, transform.position, Quaternion.identity);
        shot.Initialize(shotDirection, shotSpeed, shotRange);
    }
}
