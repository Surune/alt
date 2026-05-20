using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Boss : Enemy
{
    private enum BossPattern
    {
        AimedBurst,
        RadialNova,
        SpiralSweep
    }

    [SerializeField] private EnemyProjectile shotPrefab;
    [SerializeField] private float awarenessRange = 12f;
    [SerializeField] private float preferredDistance = 6f;
    [SerializeField] private float distanceSlack = 1.5f;
    [SerializeField] private float retreatStep = 3f;
    [SerializeField] private float strafeDistance = 2f;
    [SerializeField] private float strafeFrequency = 1.5f;
    [SerializeField] private float repathInterval = 0.2f;
    [SerializeField] private float turnSpeed = 720f;
    [SerializeField] private float patternCooldown = 2f;
    [SerializeField] private float shotSpeed = 6f;
    [SerializeField] private float shotRange = 18f;
    [SerializeField] private int aimedBurstWaveCount = 3;
    [SerializeField] private int aimedBurstShotCount = 5;
    [SerializeField] private float aimedBurstSpreadAngle = 42f;
    [SerializeField] private float aimedBurstWaveInterval = 0.3f;
    [SerializeField] private int radialNovaShotCount = 12;
    [SerializeField] private int radialNovaWaveCount = 2;
    [SerializeField] private float radialNovaWaveInterval = 0.45f;
    [SerializeField] private int spiralSweepStepCount = 10;
    [SerializeField] private int spiralSweepShotCount = 3;
    [SerializeField] private float spiralSweepSpacingAngle = 18f;
    [SerializeField] private float spiralSweepRotationPerStep = 24f;
    [SerializeField] private float spiralSweepStepInterval = 0.12f;

    private float nextRepathTime;
    private float nextPatternTime;
    private float nextPatternStepTime;
    private float spiralAngle;
    private int patternStepIndex;
    private int nextPatternIndex;
    private bool isPatternActive;
    private BossPattern activePattern;

    private void Update()
    {
        if (!GameStateManager.Instance.IsGameplayActive)
        {
            return;
        }

        UpdateFacing();
        UpdateMovement();
        UpdatePattern();
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

    private void UpdatePattern()
    {
        if (!isPatternActive)
        {
            if (Time.time < nextPatternTime)
            {
                return;
            }

            StartNextPattern();
        }

        if (Time.time < nextPatternStepTime)
        {
            return;
        }

        switch (activePattern)
        {
            case BossPattern.AimedBurst:
                ExecuteAimedBurst();
                break;
            case BossPattern.RadialNova:
                ExecuteRadialNova();
                break;
            case BossPattern.SpiralSweep:
            default:
                ExecuteSpiralSweep();
                break;
        }
    }

    private void StartNextPattern()
    {
        activePattern = (BossPattern)(nextPatternIndex % 3);
        nextPatternIndex++;
        patternStepIndex = 0;
        isPatternActive = true;
        nextPatternStepTime = Time.time;
    }

    private void ExecuteAimedBurst()
    {
        FireSpread(GetDirectionToPlayer(), aimedBurstShotCount, aimedBurstSpreadAngle);
        patternStepIndex++;

        if (patternStepIndex >= aimedBurstWaveCount)
        {
            FinishPattern();
            return;
        }

        nextPatternStepTime = Time.time + aimedBurstWaveInterval;
    }

    private void ExecuteRadialNova()
    {
        FireRadial(radialNovaShotCount, spiralAngle);
        spiralAngle += 360f / (radialNovaShotCount * 2f);
        patternStepIndex++;

        if (patternStepIndex >= radialNovaWaveCount)
        {
            FinishPattern();
            return;
        }

        nextPatternStepTime = Time.time + radialNovaWaveInterval;
    }

    private void ExecuteSpiralSweep()
    {
        FireSpread(GetDirectionFromAngle(spiralAngle), spiralSweepShotCount, spiralSweepSpacingAngle);
        spiralAngle += spiralSweepRotationPerStep;
        patternStepIndex++;

        if (patternStepIndex >= spiralSweepStepCount)
        {
            FinishPattern();
            return;
        }

        nextPatternStepTime = Time.time + spiralSweepStepInterval;
    }

    private void FinishPattern()
    {
        isPatternActive = false;
        nextPatternTime = Time.time + patternCooldown;
    }

    private Vector3 GetDirectionToPlayer()
    {
        var direction = player.position - transform.position;
        direction.y = 0f;
        return direction.normalized;
    }

    private Vector3 GetDirectionFromAngle(float angle)
    {
        return Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
    }

    private void FireSpread(Vector3 baseDirection, int shotCount, float spreadAngle)
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

    private void FireRadial(int shotCount, float startAngle)
    {
        var angleStep = 360f / shotCount;
        for (var i = 0; i < shotCount; i++)
        {
            SpawnProjectile(GetDirectionFromAngle(startAngle + (angleStep * i)));
        }
    }

    private void SpawnProjectile(Vector3 shotDirection)
    {
        var shot = Instantiate(shotPrefab, transform.position, Quaternion.identity);
        shot.Initialize(shotDirection.normalized, shotSpeed, shotRange);
    }
}
