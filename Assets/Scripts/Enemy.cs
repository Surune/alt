using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
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
    private int bloodDropAmount;
    private int activeSupportBuffCount;
    private readonly List<Material> highlightMaterials = new();
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Awake()
    {
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
        maxHealth = baseMaxHealth;
        currentHealth = maxHealth;
        contactDamage = baseContactDamage;
        agent.speed = baseMoveSpeed;
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (!(currentHealth <= 0))
        {
            return;
        }

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
    
    private void SpawnBloodPickup(int amount)
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
        if (highlightMaterials.Count > 0)
        {
            for (var i = 0; i < highlightMaterials.Count; i++)
            {
                highlightMaterials[i].EnableKeyword("_EMISSION");
                highlightMaterials[i].SetColor(EmissionColor, Color.yellow * 2f);
            }

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
                materialInstance.SetColor(EmissionColor, Color.yellow * 2f);
                materials[j] = materialInstance;
                highlightMaterials.Add(materialInstance);
            }

            renderers[i].materials = materials;
        }
    }

    private void DisableHighlight()
    {
        for (var i = 0; i < highlightMaterials.Count; i++)
        {
            highlightMaterials[i].SetColor(EmissionColor, Color.black);
        }
    }

    private void OnDestroy()
    {
        for (var i = 0; i < highlightMaterials.Count; i++)
        {
            Destroy(highlightMaterials[i]);
        }
    }
}
