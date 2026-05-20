using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    private const int RoundsPerStage = 7;

    [SerializeField] private EnemyData[] enemyCatalog;
    [SerializeField] private float roundStartDelay = 0.75f;
    [SerializeField] private float roundDuration = 15f;
    [SerializeField] private float spawnInnerRadius = 3.5f;
    [SerializeField] private float spawnOuterRadius = 4.75f;

    private int currentStage = 1;
    private int currentRound = 1;
    private int aliveEnemyCount;

    private void OnEnable()
    {
        Enemy.OnDeath += HandleAgentOnDeath;
    }

    private void Start()
    {
        StartCoroutine(RunRounds());
    }

    private void OnDisable()
    {
        Enemy.OnDeath -= HandleAgentOnDeath;
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
        var wave = GetCurrentWave();
        var roundSquare = currentRound * currentRound;
        var spawnCount = roundSquare + 1;
        var availableEnemies = GetAvailableEnemies(wave);

        for (var i = 0; i < spawnCount; i++)
        {
            var spawnPosition = GetSpawnPosition(i, spawnCount);
            var enemyData = availableEnemies[(wave + i) % availableEnemies.Count];
            var enemy = Instantiate(enemyData.Prefab, spawnPosition, Quaternion.identity);
            enemy.Initialize(enemyData, wave);
            aliveEnemyCount++;
        }

        Debug.Log($"Stage {currentStage} Round {currentRound} started: {spawnCount} enemies / wave {wave}");
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

    private int GetCurrentWave()
    {
        return ((currentStage - 1) * RoundsPerStage) + currentRound;
    }

    private List<EnemyData> GetAvailableEnemies(int wave)
    {
        var availableEnemies = new List<EnemyData>();

        for (var i = 0; i < enemyCatalog.Length; i++)
        {
            var enemyData = enemyCatalog[i];
            if (enemyData.StartWave <= wave)
            {
                availableEnemies.Add(enemyData);
            }
        }

        return availableEnemies;
    }

    private void HandleAgentOnDeath(Enemy deadEnemy)
    {
        if (deadEnemy is not Enemy_Baby)
        {
            return;
        }

        aliveEnemyCount--;
    }
}
