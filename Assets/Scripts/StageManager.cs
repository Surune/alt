using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField] private EnemyData[] enemyCatalog;
    [SerializeField] private float roundStartDelay = 0.75f;
    [SerializeField] private float[] roundDurations = { 15f };
    [SerializeField] private float spawnInnerRadius = 3.5f;
    [SerializeField] private float spawnOuterRadius = 4.75f;

    private int currentRound = 1;
    private int aliveEnemyCount;

    private void OnEnable()
    {
        Enemy.OnSpawned += HandleEnemySpawned;
        Enemy.OnDeath += HandleAgentOnDeath;
    }

    private void Start()
    {
        StartCoroutine(RunRounds());
    }

    private void OnDisable()
    {
        Enemy.OnSpawned -= HandleEnemySpawned;
        Enemy.OnDeath -= HandleAgentOnDeath;
    }

    private IEnumerator RunRounds()
    {
        yield return new WaitForSeconds(roundStartDelay);

        var totalRounds = roundDurations.Length;

        while (currentRound <= totalRounds)
        {
            StartRound();
            yield return WaitForRoundEnd();
            AdvanceRound();

            if (currentRound <= totalRounds)
            {
                UIManager.Instance.ShowPopupCard();
                yield return new WaitUntil(() => !UIManager.Instance.IsPopupCardOpen);
            }
        }

        Debug.Log($"All {totalRounds} rounds completed.");
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

        Debug.Log($"Round {currentRound} started: {spawnCount} enemies / wave {wave}");
    }

    private IEnumerator WaitForRoundEnd()
    {
        var elapsed = 0f;
        var roundDuration = GetCurrentRoundDuration();

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
    }

    private float GetCurrentRoundDuration()
    {
        return roundDurations[currentRound - 1];
    }

    private int GetCurrentWave()
    {
        return currentRound;
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
        aliveEnemyCount--;
    }

    private void HandleEnemySpawned(Enemy spawnedEnemy)
    {
        aliveEnemyCount++;
    }
}
