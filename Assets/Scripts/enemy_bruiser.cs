using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy_Bruiser : Enemy
{
    [SerializeField] private float awarenessRange = 16f;
    [SerializeField] private float chargeTriggerRange = 6.5f;
    [SerializeField] private float chargeWindupTime = 0.9f;
    [SerializeField] private float chargeDuration = 0.65f;
    [SerializeField] private float chargeCooldown = 1.1f;
    [SerializeField] private float chargeSpeed = 12f;
    [SerializeField] private float chargeOvershootDistance = 2f;
    [SerializeField] private float repathInterval = 0.2f;
    [SerializeField] private float turnSpeed = 540f;

    private float moveSpeed;
    private float nextRepathTime;
    private float stateEndTime;
    private Vector3 chargeDestination;
    private BruiserState state;

    private enum BruiserState
    {
        Chase,
        Windup,
        Charge,
        Cooldown
    }

    private void Start()
    {
        moveSpeed = agent.speed;
    }

    private void Update()
    {
        if (!GameStateManager.Instance.IsGameplayActive)
        {
            return;
        }

        switch (state)
        {
            case BruiserState.Chase:
                UpdateFacing(player.position);
                UpdateChase();
                break;
            case BruiserState.Windup:
                UpdateFacing(player.position);
                UpdateWindup();
                break;
            case BruiserState.Charge:
                UpdateFacing(chargeDestination);
                UpdateCharge();
                break;
            case BruiserState.Cooldown:
                UpdateFacing(player.position);
                UpdateCooldown();
                break;
        }
    }

    private void UpdateFacing(Vector3 targetPosition)
    {
        var direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        var targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    private void UpdateChase()
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

        if (distance <= chargeTriggerRange)
        {
            BeginWindup(toPlayer, distance);
            return;
        }

        agent.speed = moveSpeed;

        var destination = player.position;
        if (NavMesh.SamplePosition(destination, out var hit, 4f, NavMesh.AllAreas))
        {
            destination = hit.position;
        }

        agent.SetDestination(destination);
    }

    private void BeginWindup(Vector3 toPlayer, float distance)
    {
        state = BruiserState.Windup;
        stateEndTime = Time.time + chargeWindupTime;
        agent.ResetPath();
        agent.speed = moveSpeed;

        var chargeDirection = toPlayer / distance;
        chargeDestination = player.position + (chargeDirection * chargeOvershootDistance);
        if (NavMesh.SamplePosition(chargeDestination, out var hit, 4f, NavMesh.AllAreas))
        {
            chargeDestination = hit.position;
        }
    }

    private void UpdateWindup()
    {
        if (Time.time < stateEndTime)
        {
            return;
        }

        state = BruiserState.Charge;
        stateEndTime = Time.time + chargeDuration;
        agent.speed = chargeSpeed;
        agent.SetDestination(chargeDestination);
    }

    private void UpdateCharge()
    {
        if (Time.time < stateEndTime)
        {
            return;
        }

        state = BruiserState.Cooldown;
        stateEndTime = Time.time + chargeCooldown;
        agent.ResetPath();
        agent.speed = moveSpeed;
    }

    private void UpdateCooldown()
    {
        if (Time.time < stateEndTime)
        {
            return;
        }

        state = BruiserState.Chase;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerMover>(out var playerMover))
        {
            playerMover.TakeDamage(contactDamage);
        }
    }
}
