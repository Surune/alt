using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private Wing wingPrefab;
    [SerializeField] private BloodPickup bloodPickupPrefab;
    [SerializeField] private PlayerProjectile playerPrefab;
    [SerializeField] private EnemyProjectile enemyProjectilePrefab;

    public GameStateManager GameState { get; private set; }
    public PoolManager Pool { get; private set; }
    public AbilityManager Ability { get; private set; }

    private void Awake()
    {
        Instance = this;
        GameState = new GameStateManager();
        Pool = new PoolManager(bloodPickupPrefab, playerPrefab, enemyProjectilePrefab);
        Ability = new AbilityManager(FindFirstObjectByType<Player>(), wingPrefab);
    }

    private void Update()
    {
        Ability.Tick();
    }

    private void OnDestroy()
    {
        Ability.Dispose();
        Pool.Dispose();
        GameState.Dispose();
        Instance = null;
    }
}
