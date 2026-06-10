using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    public static event Action<Enemy> OnSpawned;
    public static event Action<Enemy> OnDeath;

    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] private BloodPickup bloodPickupPrefab;

    protected Transform player;
    protected float maxHealth;
    protected float currentHealth;
    protected float contactDamage;
    protected int currentWave;
    protected float baseMaxHealth;
    protected float baseContactDamage;
    protected float baseMoveSpeed;
    protected int bloodDropAmount;
    private int activeSupportBuffCount;
    private readonly List<Material> highlightMaterials = new();
    private readonly List<Color> baseColors = new();
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    private static readonly List<Enemy> ActiveEnemies = new();
    private const float DamageFlashDuration = 0.1f;

    public bool IsFullHealth => currentHealth >= maxHealth;
    public bool IsDead => currentHealth <= 0f;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Awake()
    {
        ActiveEnemies.Add(this);
        player = FindFirstObjectByType<Player>().transform;
        agent.baseOffset = 0f;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    public void Initialize(EnemyData enemyData, int currentWave)
    {
        this.currentWave = currentWave;
        var waveOffset = currentWave - enemyData.StartWave;
        baseMaxHealth = enemyData.MaxHealth + (enemyData.HealthIncreasePerWave * waveOffset);
        bloodDropAmount = enemyData.BloodDropAmount;
        baseContactDamage = enemyData.Damage + (enemyData.DamageIncreasePerWave * waveOffset);
        baseMoveSpeed = enemyData.MoveSpeed;
        var abilityManager = GameManager.Instance.Ability;
        baseMaxHealth += abilityManager.EnemyHealthOffset;
        baseContactDamage *= abilityManager.EnemyDamageCoefficient;
        baseMoveSpeed *= abilityManager.EnemySpeedCoefficient;
        maxHealth = baseMaxHealth;
        currentHealth = maxHealth;
        contactDamage = baseContactDamage;
        agent.speed = baseMoveSpeed;

        if (abilityManager.SpawnSmall)
        {
            transform.localScale *= 0.75f;
        }

        if (abilityManager.SpawnRandom)
        {
            var randomScale = Random.Range(0.65f, 1.35f);
            transform.localScale *= randomScale;
            maxHealth *= randomScale;
            currentHealth = maxHealth;
        }
    }

    public virtual void TakeDamage(float damage)
    {
        PlayDamageFlash();
        currentHealth -= damage;

        if (!(currentHealth <= 0))
        {
            return;
        }

        Die();
    }

    public void TakeFatalDamage(float bossDamage)
    {
        TakeDamage(this is Boss ? maxHealth * bossDamage : maxHealth);
    }

    public void Slow(float coefficient, float duration)
    {
        StopCoroutine(nameof(RestoreSpeed));
        agent.speed = baseMoveSpeed * coefficient;
        StartCoroutine(nameof(RestoreSpeed), duration);
    }

    private IEnumerator RestoreSpeed(float duration)
    {
        yield return new WaitForSeconds(duration);
        agent.speed = baseMoveSpeed;
    }

    public static void DamageAll(float damage, Enemy excludedEnemy = null)
    {
        var snapshot = ActiveEnemies.ToArray();
        for (var i = 0; i < snapshot.Length; i++)
        {
            if (snapshot[i] != excludedEnemy)
            {
                snapshot[i].TakeDamage(damage);
            }
        }
    }

    public static void RemoveAll()
    {
        var snapshot = ActiveEnemies.ToArray();
        for (var i = 0; i < snapshot.Length; i++)
        {
            Destroy(snapshot[i].gameObject);
        }
    }

    public static void SlowAll(float coefficient, float duration)
    {
        for (var i = 0; i < ActiveEnemies.Count; i++)
        {
            ActiveEnemies[i].Slow(coefficient, duration);
        }
    }

    public static void PullAllTowards(Vector3 position, float distance)
    {
        for (var i = 0; i < ActiveEnemies.Count; i++)
        {
            var enemy = ActiveEnemies[i];
            var direction = position - enemy.transform.position;
            direction.y = 0f;
            enemy.agent.Warp(enemy.transform.position + direction.normalized * Mathf.Min(distance, direction.magnitude));
        }
    }

    public static Vector3 GetNearestPosition(Vector3 position)
    {
        var nearestPosition = position + Vector3.forward;
        var nearestDistance = float.MaxValue;
        for (var i = 0; i < ActiveEnemies.Count; i++)
        {
            var enemyPosition = ActiveEnemies[i].transform.position;
            var distance = (enemyPosition - position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPosition = enemyPosition;
            }
        }

        return nearestPosition;
    }

    protected void NotifySpawned(Enemy enemy)
    {
        OnSpawned?.Invoke(enemy);
    }

    protected void Die()
    {
        SpawnBloodPickup(bloodDropAmount);
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }

    public void RestoreHealth(float amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
    
    public void AddSupportBuff()
    {
        activeSupportBuffCount++;
        RefreshSupportBuff();
    }

    public void RemoveSupportBuff()
    {
        activeSupportBuffCount--;
        RefreshSupportBuff();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Player>(out var playerMover))
        {
            playerMover.TakeDamage(contactDamage);
        }
    }
    
    protected void SpawnBloodPickup(int amount)
    {
        var bloodPosition = transform.position;
        var bloodRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        BloodPickup.SpawnOrGrow(bloodPickupPrefab, bloodPosition, bloodRotation, amount);
    }

    private void RefreshSupportBuff()
    {
        var wasBuffed = maxHealth > baseMaxHealth;
        var healthRatio = currentHealth / maxHealth;

        if (activeSupportBuffCount > 0)
        {
            maxHealth = baseMaxHealth * 1.5f;
            contactDamage = baseContactDamage * 1.25f;
            agent.speed = baseMoveSpeed * 1.5f;
        }
        else
        {
            maxHealth = baseMaxHealth;
            contactDamage = baseContactDamage;
            agent.speed = baseMoveSpeed;
        }

        currentHealth = maxHealth * healthRatio;

        if (!wasBuffed && activeSupportBuffCount > 0)
        {
            EnableHighlight();
            return;
        }

        if (wasBuffed && activeSupportBuffCount <= 0)
        {
            DisableHighlight();
        }
    }

    private void EnableHighlight()
    {
        EnsureHighlightMaterials();
        SetHighlightColor(Color.yellow * 2f);
    }

    protected void PlayDamageFlash()
    {
        StopCoroutine(nameof(RestoreBaseColors));
        EnsureHighlightMaterials();
        SetBaseColor(Color.red);
        StartCoroutine(nameof(RestoreBaseColors));
    }

    private void EnsureHighlightMaterials()
    {
        if (highlightMaterials.Count > 0)
        {
            return;
        }

        var renderers = GetComponentsInChildren<Renderer>();
        for (var i = 0; i < renderers.Length; i++)
        {
            var materials = renderers[i].materials;
            for (var j = 0; j < materials.Length; j++)
            {
                var materialInstance = Instantiate(materials[j]);
                materialInstance.EnableKeyword("_EMISSION");
                materialInstance.SetColor(EmissionColor, Color.black);
                materials[j] = materialInstance;
                highlightMaterials.Add(materialInstance);
                baseColors.Add(materialInstance.GetColor(BaseColor));
            }

            renderers[i].materials = materials;
        }
    }

    private IEnumerator RestoreBaseColors()
    {
        yield return new WaitForSeconds(DamageFlashDuration);
        for (var i = 0; i < highlightMaterials.Count; i++)
        {
            highlightMaterials[i].SetColor(BaseColor, baseColors[i]);
        }
    }

    private void SetBaseColor(Color color)
    {
        for (var i = 0; i < highlightMaterials.Count; i++)
        {
            highlightMaterials[i].SetColor(BaseColor, color);
        }
    }

    private void SetHighlightColor(Color color)
    {
        for (var i = 0; i < highlightMaterials.Count; i++)
        {
            highlightMaterials[i].EnableKeyword("_EMISSION");
            highlightMaterials[i].SetColor(EmissionColor, color);
        }
    }

    private void DisableHighlight()
    {
        SetHighlightColor(Color.black);
    }

    private void OnDestroy()
    {
        ActiveEnemies.Remove(this);
        for (var i = 0; i < highlightMaterials.Count; i++)
        {
            Destroy(highlightMaterials[i]);
        }
    }
}
