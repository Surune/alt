using UnityEngine;
using UnityEngine.AI;

public class Enemy_Spitter : Enemy
{
    [SerializeField] private float awarenessRange = 14f;
    [SerializeField] private float preferredDistance = 7f;
    [SerializeField] private float distanceSlack = 1.5f;
    [SerializeField] private float retreatDistance = 4f;
    [SerializeField] private float approachOffset = 1.5f;
    [SerializeField] private float strafeDistance = 1.5f;
    [SerializeField] private float strafeFrequency = 1.2f;
    [SerializeField] private float repathInterval = 0.2f;
    [SerializeField] private float turnSpeed = 720f;
    [SerializeField] private float fireInterval = 1.4f;
    [SerializeField] private int shotCount = 3;
    [SerializeField] private float spreadAngle = 18f;

    private float nextRepathTime;
    private float nextShotTime;

    private void Update()
    {
        if (!GameManager.Instance.GameState.IsGameplayActive)
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
        FireSpread(toPlayer.normalized);
    }

    private void FireSpread(Vector3 baseDirection)
    {
        if (shotCount == 1)
        {
            SpawnProjectile(baseDirection);
            return;
        }

        for (var i = 0; i < shotCount; i++)
        {
            var t = i / (float)(shotCount - 1);
            var angle = Mathf.Lerp(-spreadAngle * 0.5f, spreadAngle * 0.5f, t);
            var shotDirection = Quaternion.AngleAxis(angle, Vector3.up) * baseDirection;
            SpawnProjectile(shotDirection);
        }
    }

    private void SpawnProjectile(Vector3 shotDirection)
    {
        var shot = GameManager.Instance.Pool.GetEnemyProjectile(transform.position, Quaternion.identity);
        shot.Initialize(shotDirection.normalized);
        shot.gameObject.SetActive(true);
    }
}
