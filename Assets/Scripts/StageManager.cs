using System.Collections;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    private const int RoundsPerStage = 7;

    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private float roundStartDelay = 0.75f;
    [SerializeField] private float roundDuration = 15f;
    [SerializeField] private float spawnInnerRadius = 3.5f;
    [SerializeField] private float spawnOuterRadius = 4.75f;

    private int currentStage = 1;
    private int currentRound = 1;
    private int aliveEnemyCount;

    private void OnEnable()
    {
        Agent.Died += HandleAgentDied;
    }

    private void Start()
    {
        StartCoroutine(RunRounds());
    }

    private void OnDisable()
    {
        Agent.Died -= HandleAgentDied;
    }

    private IEnumerator RunRounds()
    {
        yield return new WaitForSeconds(roundStartDelay);

        while (true)
        {
            StartRound();
            yield return WaitForRoundEnd();
            AdvanceRound();
        }
    }

    private void StartRound()
    {
        var roundSquare = currentRound * currentRound;
        var maxHealth = currentStage + roundSquare;
        var spawnCount = roundSquare + 1;

        for (var i = 0; i < spawnCount; i++)
        {
            var spawnPosition = GetSpawnPosition(i, spawnCount);
            var enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            enemy.Initialize(maxHealth);
            aliveEnemyCount++;
        }

        Debug.Log($"Stage {currentStage} Round {currentRound} started: {spawnCount} enemies / {maxHealth} HP");
    }

    private IEnumerator WaitForRoundEnd()
    {
        var elapsed = 0f;

        while (elapsed < roundDuration)
        {
            if (aliveEnemyCount <= 0)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private Vector3 GetSpawnPosition(int spawnIndex, int spawnCount)
    {
        var normalizedIndex = spawnIndex / (float)spawnCount;
        var angle = normalizedIndex * Mathf.PI * 2f;
        var radiusT = (spawnIndex % 5) / 4f;
        var radius = Mathf.Lerp(spawnInnerRadius, spawnOuterRadius, radiusT);

        var spawnPosition = Vector3.zero;
        spawnPosition.x = Mathf.Cos(angle) * radius;
        spawnPosition.z = Mathf.Sin(angle) * radius;
        return spawnPosition;
    }

    private void AdvanceRound()
    {
        currentRound++;
        if (currentRound <= RoundsPerStage)
        {
            return;
        }

        currentStage++;
        currentRound = 1;
    }

    private void HandleAgentDied(Agent deadAgent)
    {
        if (deadAgent is not Enemy)
        {
            return;
        }

        aliveEnemyCount--;
    }
}
