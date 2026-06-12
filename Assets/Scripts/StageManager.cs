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

        availableEnemies = GetAvailableEnemies(currentRound);
        Debug.Log($"Round {currentRound} started");
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
                if (!GameManager.Instance.Ability.DisableEnemySpawning)
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
        var spawnPosition = GetSpawnPosition();
        var enemyData = availableEnemies.PickRandom();
        var enemy = Instantiate(enemyData.Prefab, spawnPosition, Quaternion.identity);
        enemy.Initialize(enemyData, currentWave);
        aliveEnemyCount++;
    }

    private Vector3 GetSpawnPosition()
    {
        var spawnPosition = Vector3.zero;
        var randomPosition = Random.insideUnitCircle * Random.Range(spawnInnerRadius, spawnOuterRadius);
        spawnPosition.x = randomPosition.x;
        spawnPosition.z = randomPosition.y;
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

    private List<EnemyData> GetAvailableEnemies(int round)
    {
        return enemyCatalog.Where(enemyData => enemyData.StartWave <= round).ToList();
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
