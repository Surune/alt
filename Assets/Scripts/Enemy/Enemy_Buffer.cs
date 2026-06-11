using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_Buffer : Enemy
{
    [SerializeField] private float awarenessRange = 18f;
    [SerializeField] private float fleeTriggerRange = 7f;
    [SerializeField] private float fleeDistance = 6f;
    [SerializeField] private float supportRange = 6f;
    [SerializeField] private float allyAnchorRange = 14f;
    [SerializeField] private float allyOffsetRadius = 2.5f;
    [SerializeField] private float repathInterval = 0.25f;
    [SerializeField] private float retargetInterval = 1f;
    [SerializeField] private float buffInterval = 0.25f;
    [SerializeField] private float turnSpeed = 540f;

    private readonly List<Enemy> buffedEnemies = new();
    private float nextRepathTime;
    private float nextRetargetTime;
    private float nextBuffTime;
    private Enemy anchorEnemy;

    private void OnEnable()
    {
        Enemy.OnDeath += HandleEnemyDeath;
    }

    private void OnDisable()
    {
        Enemy.OnDeath -= HandleEnemyDeath;
        ClearBuffs();
    }

    private void Update()
    {
        if (!GameManager.Instance.GameState.IsGameplayActive)
        {
            return;
        }

        UpdateAnchorEnemy();
        UpdateFacing();
        UpdateMovement();
        UpdateBuffs();
    }

    private void UpdateAnchorEnemy()
    {
        if (Time.time < nextRetargetTime)
        {
            return;
        }

        nextRetargetTime = Time.time + retargetInterval;

        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        var closestSqrDistance = allyAnchorRange * allyAnchorRange;
        Enemy closestEnemy = this;

        foreach (var enemy in enemies)
        {
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

        anchorEnemy = closestEnemy;
    }

    private void UpdateFacing()
    {
        var toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        var playerDistance = toPlayer.magnitude;

        var focusPosition = anchorEnemy.transform.position;
        if (playerDistance <= fleeTriggerRange && playerDistance > 0.001f)
        {
            focusPosition = transform.position - toPlayer;
        }

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

        var toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        var playerDistance = toPlayer.magnitude;

        if (playerDistance > awarenessRange && anchorEnemy == this)
        {
            agent.ResetPath();
            return;
        }

        var destination = transform.position;

        if (playerDistance <= fleeTriggerRange && playerDistance > 0.001f)
        {
            destination -= toPlayer / playerDistance * fleeDistance;
        }
        else if (anchorEnemy != this)
        {
            var toAnchor = transform.position - anchorEnemy.transform.position;
            toAnchor.y = 0f;

            if (toAnchor.sqrMagnitude <= 0.001f)
            {
                toAnchor = transform.right;
                toAnchor.y = 0f;
            }

            destination = anchorEnemy.transform.position + (toAnchor.normalized * allyOffsetRadius);
        }

        if (NavMesh.SamplePosition(destination, out var hit, 4f, NavMesh.AllAreas))
        {
            destination = hit.position;
        }

        agent.SetDestination(destination);
    }

    private void UpdateBuffs()
    {
        if (Time.time < nextBuffTime)
        {
            return;
        }

        nextBuffTime = Time.time + buffInterval;

        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        var rangeSqr = supportRange * supportRange;

        for (var i = buffedEnemies.Count - 1; i >= 0; i--)
        {
            var buffedEnemy = buffedEnemies[i];
            var keepBuff = false;

            for (var j = 0; j < enemies.Length; j++)
            {
                if (enemies[j] != buffedEnemy)
                {
                    continue;
                }

                var offset = buffedEnemy.transform.position - transform.position;
                offset.y = 0f;
                keepBuff = offset.sqrMagnitude <= rangeSqr;
                break;
            }

            if (keepBuff)
            {
                continue;
            }

            buffedEnemy.RemoveSupportBuff();
            buffedEnemies.RemoveAt(i);
        }

        for (var i = 0; i < enemies.Length; i++)
        {
            var enemy = enemies[i];
            if (enemy == this)
            {
                continue;
            }

            var offset = enemy.transform.position - transform.position;
            offset.y = 0f;

            if (offset.sqrMagnitude > rangeSqr || buffedEnemies.Contains(enemy))
            {
                continue;
            }

            enemy.AddSupportBuff();
            buffedEnemies.Add(enemy);
        }
    }

    private void ClearBuffs()
    {
        for (var i = 0; i < buffedEnemies.Count; i++)
        {
            buffedEnemies[i].RemoveSupportBuff();
        }

        buffedEnemies.Clear();
    }

    private void HandleEnemyDeath(Enemy deadEnemy)
    {
        var index = buffedEnemies.IndexOf(deadEnemy);
        if (index >= 0)
        {
            buffedEnemies.RemoveAt(index);
        }

        if (deadEnemy == anchorEnemy)
        {
            anchorEnemy = this;
            nextRetargetTime = 0f;
        }
    }
}
