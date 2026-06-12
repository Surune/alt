using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    [SerializeField] private EnemyData[] enemyCatalog;
    [SerializeField] private float roundStartDelay = 0.75f;
    [SerializeField] private float cardPopupDelay = 1f;
    [SerializeField] private float[] roundDurations = { 15f };
    [SerializeField] private float spawnInnerRadius = 3.5f;
    [SerializeField] private float spawnOuterRadius = 4.75f;
    [SerializeField] private float minSpawnInterval = 0.2f;
    [SerializeField] private float maxSpawnInterval = 0.5f;

    private int currentRound = 1;
    private int aliveEnemyCount;
    private int spawnTargetCount;
    private int nextSpawnIndex;
    private int currentWave;
    private List<EnemyData> availableEnemies;
    private Player player;
    private TimePanel timePanel;

    private void Awake()
    {
        player = FindFirstObjectByType<Player>();
        timePanel = FindFirstObjectByType<TimePanel>();
    }

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

            if (currentRound < totalRounds)
            {
                GameManager.Instance.GameState.EnterRoundTransitionState();
            }

            BloodPickup.CollectAll(player);
            AdvanceRound();

            if (currentRound <= totalRounds)
            {
                yield return new WaitForSeconds(cardPopupDelay);
                UIManager.Instance.ShowPopupCard();

                if (!UIManager.Instance.IsPopupCardOpen)
                {
                    GameManager.Instance.GameState.EnterPlayingState();
                }

                yield return new WaitUntil(() => !UIManager.Instance.IsPopupCardOpen);
            }
        }

        Debug.Log($"All {totalRounds} rounds completed.");
        SceneManager.LoadScene("GameClearScene");
    }

    private void StartRound()
    {
        timePanel.UpdateDisplay(currentRound, GetCurrentRoundDuration());

        if (GameManager.Instance.Ability.DisableEnemySpawning)
        {
            return;
        }

        currentWave = GetCurrentWave();
        var roundSquare = currentRound * currentRound;
        spawnTargetCount = roundSquare + 1 + GameManager.Instance.Ability.AdditionalEnemyCount;
        availableEnemies = GetAvailableEnemies(currentWave);
        nextSpawnIndex = 0;

        Debug.Log($"Round {currentRound} started: {spawnTargetCount} spawn target / wave {currentWave}");
    }

    private IEnumerator WaitForRoundEnd()
    {
        var elapsed = 0f;
        var roundDuration = GetCurrentRoundDuration();
        var nextSpawnTime = Random.Range(minSpawnInterval, maxSpawnInterval);

        while (elapsed < roundDuration)
        {
            timePanel.UpdateDisplay(currentRound, roundDuration - elapsed);

            if (elapsed >= nextSpawnTime)
            {
                if (!GameManager.Instance.Ability.DisableEnemySpawning && aliveEnemyCount < spawnTargetCount)
                {
                    SpawnEnemy();
                }

                nextSpawnTime = elapsed + Random.Range(minSpawnInterval, maxSpawnInterval);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        timePanel.UpdateDisplay(currentRound, 0f);
        Enemy.RemoveAll();
        aliveEnemyCount = 0;
    }

    private void SpawnEnemy()
    {
        var spawnPosition = GetSpawnPosition(nextSpawnIndex, spawnTargetCount);
        var enemyData = availableEnemies[(currentWave + nextSpawnIndex) % availableEnemies.Count];
        var enemy = Instantiate(enemyData.Prefab, spawnPosition, Quaternion.identity);
        enemy.Initialize(enemyData, currentWave);
        aliveEnemyCount++;
        nextSpawnIndex++;
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
        return enemyCatalog.Where(enemyData => enemyData.StartWave <= wave).ToList();
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
