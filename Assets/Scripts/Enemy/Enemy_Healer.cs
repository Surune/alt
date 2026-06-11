using UnityEngine;
using UnityEngine.AI;

public class Enemy_Healer : Enemy
{
    [SerializeField] private float trackingRange = 18f;
    [SerializeField] private float healRange = 5.5f;
    [SerializeField] private float healInterval = 4f;
    [SerializeField] private float roamRadius = 3f;
    [SerializeField] private float repathInterval = 0.35f;
    [SerializeField] private float retargetInterval = 1.2f;
    [SerializeField] private float turnSpeed = 540f;

    private float nextHealTime;
    private float nextRepathTime;
    private float nextRetargetTime;
    private Enemy trackedEnemy;
    private Vector3 roamOffset;

    private void Update()
    {
        if (!GameManager.Instance.GameState.IsGameplayActive)
        {
            return;
        }

        UpdateTrackedEnemy();
        UpdateFacing();
        UpdateMovement();
        UpdateHeal();
    }

    private void UpdateTrackedEnemy()
    {
        if (Time.time < nextRetargetTime)
        {
            return;
        }

        nextRetargetTime = Time.time + retargetInterval;

        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        var closestSqrDistance = trackingRange * trackingRange;
        Enemy closestEnemy = this;

        for (var i = 0; i < enemies.Length; i++)
        {
            var enemy = enemies[i];
            if (enemy == this)
            {
                continue;
            }

            var offset = enemy.transform.position - transform.position;
            offset.y = 0f;
            var sqrDistance = offset.sqrMagnitude;

            if (sqrDistance >= closestSqrDistance)
            {
                continue;
            }

            closestSqrDistance = sqrDistance;
            closestEnemy = enemy;
        }

        trackedEnemy = closestEnemy;
        roamOffset = Random.insideUnitSphere * roamRadius;
        roamOffset.y = 0f;
    }

    private void UpdateFacing()
    {
        var focusPosition = trackedEnemy.transform.position;
        var direction = focusPosition - transform.position;
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

        var destination = trackedEnemy.transform.position + roamOffset;
        if (NavMesh.SamplePosition(destination, out var hit, 4f, NavMesh.AllAreas))
        {
            destination = hit.position;
        }

        agent.SetDestination(destination);
    }

    private void UpdateHeal()
    {
        if (Time.time < nextHealTime)
        {
            return;
        }

        nextHealTime = Time.time + healInterval;

        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        var amount = 100f + (10f * currentWave);
        var rangeSqr = healRange * healRange;

        foreach (var enemy in enemies)
        {
            if (enemy == this)
            {
                continue;
            }

            var offset = enemy.transform.position - transform.position;
            offset.y = 0f;

            if (offset.sqrMagnitude > rangeSqr)
            {
                continue;
            }

            enemy.RestoreHealth(amount);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Player>(out var playerMover))
        {
            playerMover.TakeDamage(contactDamage);
        }
    }
}
