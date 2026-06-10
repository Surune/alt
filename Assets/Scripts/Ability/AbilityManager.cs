using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance { get; private set; }

    [HideInInspector] public int[] Synergy;
    public Color[] SynergyColors;
    [SerializeField] Wing wingPrefab;

    public bool assassination = false;
    public bool penetrate = false;
    public bool luckySeven = false;
    public bool beingstronger = false;
    public bool third = false;
    public bool nuker = false;
    public bool culling = false;
    public bool noxious = false;
    public bool psychosense = false;
    public bool psychosink = false;
    public bool freezing = false;
    public bool locked = false;
    public bool awaken = false;
    public bool burst = false;
    public bool echo = false;
    public bool immolation = false;
    public bool explode = false;
    public bool magnet = false;
    public bool burning = false;
    public bool kineticBarrage = false;
    public bool udo = false;
    public bool fifth = false;
    public bool fracture = false;
    public bool firm = false;

    AbilityData[] abilityCatalog;
    readonly List<AbilityData> selectedAbilities = new();
    readonly HashSet<int> selectedAbilityIds = new();
    readonly Dictionary<int, float> periodicIntervals = new();
    readonly Dictionary<int, float> nextPeriodicTimes = new();
    Dictionary<AbilityManagerFlag, Action> flagSetters;
    Player player;
    float damage;
    float damageCoefficient = 1f;
    float criticalProbability;
    float criticalCoefficient = 1.5f;
    float fatalProbability;
    float fixedDamage;
    float shotSpeedCoefficient = 1f;
    float fireIntervalCoefficient = 1f;
    float fireIntervalOffset;
    float enemySpeedCoefficient = 1f;
    float enemyHealthOffset;
    float enemyDamageCoefficient = 1f;
    float itemProbability;
    float coinCoefficient = 1f;
    float wingDamage = 1f;
    float wingSpeed = 1f;
    float wingCooldown = 1f;
    float bossFatalDamage = 0.5f;
    float criticalProbabilityPerKill;
    float criticalDamagePerKill;
    float damageCoefficientPerKill;
    float damagePerKill;
    float damagePerWing;
    int killGrowthLimit;
    int killCount;
    int refreshCount;
    int wingCount;
    int additionalEnemyCount;
    int experienceRequirementOffset;
    bool wingHoming;
    bool wingFreezing;
    bool statikk;
    bool reinforce;
    bool jera;
    bool dagaz;
    bool aquaris;
    bool repair;
    bool hextech;
    bool disableEnemySpawning;
    bool forceEnemySpawning;
    bool spawnSmall;
    bool spawnRandom;
    bool berserk;
    bool blunt;
    bool carving;
    bool meatshield;
    bool ouroborosL;
    bool ouroborosR;
    bool porcupine;
    bool lethal;
    bool virgo;
    bool capricon;
    bool celestialShot;
    bool allTargetShot;
    bool canFire = true;
    float lastPlayerMoveDistance;
    int projectileCount;
    int criticalHitCount;

    public IReadOnlyList<AbilityData> SelectedAbilities => selectedAbilities;
    public int AbilityCount => abilityCatalog.Length;
    public int AdditionalEnemyCount => additionalEnemyCount;
    public float EnemySpeedCoefficient => enemySpeedCoefficient;
    public float EnemyHealthOffset => enemyHealthOffset;
    public float EnemyDamageCoefficient => enemyDamageCoefficient;
    public bool DisableEnemySpawning => disableEnemySpawning;
    public bool SpawnSmall => spawnSmall;
    public bool SpawnRandom => spawnRandom;
    public bool CanFire => canFire;
    public bool Awaken => awaken;
    public bool Fracture => fracture;
    public float BossFatalDamage => bossFatalDamage;
    public float WingDamage => wingDamage;
    public float WingSpeed => wingSpeed;
    public float WingCooldown => wingCooldown;
    public bool WingHoming => wingHoming;
    public bool WingFreezing => wingFreezing;

    private void Awake()
    {
        Instance = this;
        player = FindFirstObjectByType<Player>();
        abilityCatalog = Resources.LoadAll<AbilityData>("Abilities");
        Synergy = new int[11];
        CacheFlagSetters();
    }

    private void OnEnable()
    {
        Enemy.OnDeath += HandleEnemyDeath;
        BloodPickup.OnCollected += HandleItemCollected;
    }

    private void OnDisable()
    {
        Enemy.OnDeath -= HandleEnemyDeath;
        BloodPickup.OnCollected -= HandleItemCollected;
    }

    private void Update()
    {
        if (!GameStateManager.Instance.IsGameplayActive)
        {
            return;
        }

        foreach (var pair in periodicIntervals)
        {
            if (Time.time < nextPeriodicTimes[pair.Key])
            {
                continue;
            }

            nextPeriodicTimes[pair.Key] = Time.time + pair.Value;
            TriggerPeriodicAbility(pair.Key);
        }

        if (beingstronger)
        {
            lastPlayerMoveDistance += player.ConsumeMovedDistance();
        }
    }

    private void CacheFlagSetters()
    {
        flagSetters = new Dictionary<AbilityManagerFlag, Action>
        {
            { AbilityManagerFlag.Assassination, () => assassination = true },
            { AbilityManagerFlag.Penetrate, () => penetrate = true },
            { AbilityManagerFlag.LuckySeven, () => luckySeven = true },
            { AbilityManagerFlag.BeingStronger, () => beingstronger = true },
            { AbilityManagerFlag.Third, () => third = true },
            { AbilityManagerFlag.Nuker, () => nuker = true },
            { AbilityManagerFlag.Culling, () => culling = true },
            { AbilityManagerFlag.Noxious, () => noxious = true },
            { AbilityManagerFlag.Psychosense, () => psychosense = true },
            { AbilityManagerFlag.Psychosink, () => psychosink = true },
            { AbilityManagerFlag.Freezing, () => freezing = true },
            { AbilityManagerFlag.Locked, () => locked = true },
            { AbilityManagerFlag.Awaken, () => awaken = true },
            { AbilityManagerFlag.Burst, () => burst = true },
            { AbilityManagerFlag.Echo, () => echo = true },
            { AbilityManagerFlag.Immolation, () => immolation = true },
            { AbilityManagerFlag.Explode, () => explode = true },
            { AbilityManagerFlag.Magnet, () => magnet = true },
            { AbilityManagerFlag.Burning, () => burning = true },
            { AbilityManagerFlag.KineticBarrage, () => kineticBarrage = true },
            { AbilityManagerFlag.Udo, () => udo = true },
            { AbilityManagerFlag.Fifth, () => fifth = true },
            { AbilityManagerFlag.Fracture, () => fracture = true },
            { AbilityManagerFlag.Firm, () => firm = true }
        };
    }

    public AbilityData[] GetUnselectedAbilities()
    {
        var abilities = new List<AbilityData>();
        for (var i = 0; i < abilityCatalog.Length; i++)
        {
            var ability = abilityCatalog[i];
            if (!selectedAbilities.Contains(ability))
            {
                abilities.Add(ability);
            }
        }

        return abilities.ToArray();
    }

    public void AddAbility(AbilityData abilityData)
    {
        selectedAbilities.Add(abilityData);
        selectedAbilityIds.Add(abilityData.AbilityID);

        AddSynergy(abilityData.PrimarySynergy);
        AddSynergy(abilityData.SecondarySynergy);

        var ability = CreateAbility(abilityData);
        var context = new AbilityApplyContext(this, ability, abilityData, player);
        abilityData.Apply(context);
    }

    public void SetFlag(AbilityManagerFlag flag)
    {
        flagSetters[flag]();
    }

    public void ChooseRandomAbilities(int count)
    {
        for (var i = 0; i < count; i++)
        {
            var unselected = GetUnselectedAbilities();
            AddAbility(unselected[Random.Range(0, unselected.Length)]);
        }
    }

    public void RegisterPeriodicAbility(int abilityId, float interval)
    {
        periodicIntervals.Add(abilityId, interval);
        nextPeriodicTimes.Add(abilityId, Time.time + interval);
    }

    public void ChangeFireInterval(AbilityValueOperation operation, float value)
    {
        if (operation == AbilityValueOperation.Multiply)
        {
            fireIntervalCoefficient *= value;
            return;
        }

        fireIntervalOffset = operation.Apply(fireIntervalOffset, value);
    }

    public void ChangePlayerStat(PlayerManagerFloatStat stat, AbilityValueOperation operation, float value)
    {
        switch (stat)
        {
            case PlayerManagerFloatStat.Damage:
                damage = operation.Apply(damage, value);
                break;
            case PlayerManagerFloatStat.DamageCoefficient:
                damageCoefficient = operation.Apply(damageCoefficient, value);
                break;
            case PlayerManagerFloatStat.CriticalProb:
                criticalProbability = operation.Apply(criticalProbability, value);
                break;
            case PlayerManagerFloatStat.CriticalCoefficient:
                criticalCoefficient = operation.Apply(criticalCoefficient, value);
                break;
            case PlayerManagerFloatStat.FatalProb:
                fatalProbability = operation.Apply(fatalProbability, value);
                break;
            case PlayerManagerFloatStat.FixDamage:
                fixedDamage = operation.Apply(fixedDamage, value);
                break;
            case PlayerManagerFloatStat.ShotSpeedCoefficient:
                shotSpeedCoefficient = operation.Apply(shotSpeedCoefficient, value);
                break;
        }
    }

    public void SetPlayerFlag(PlayerManagerBoolStat stat, bool value)
    {
        switch (stat)
        {
            case PlayerManagerBoolStat.Statikk: statikk = value; break;
            case PlayerManagerBoolStat.Reinforce: reinforce = value; break;
            case PlayerManagerBoolStat.Jera: jera = value; break;
            case PlayerManagerBoolStat.Dagaz: dagaz = value; break;
            case PlayerManagerBoolStat.Aquaris: aquaris = value; break;
            case PlayerManagerBoolStat.Repair: repair = value; break;
        }
    }

    public void ChangeSpawnerStat(SpawnerFloatStat stat, AbilityValueOperation operation, float value)
    {
        switch (stat)
        {
            case SpawnerFloatStat.DamageCoefficient:
                enemyDamageCoefficient = operation.Apply(enemyDamageCoefficient, value);
                break;
            case SpawnerFloatStat.MeteorCoefficient:
            case SpawnerFloatStat.SpeedCoefficient:
                enemySpeedCoefficient = operation.Apply(enemySpeedCoefficient, value);
                break;
            case SpawnerFloatStat.AddHP:
                enemyHealthOffset = operation.Apply(enemyHealthOffset, value);
                break;
        }
    }

    public void SetSpawnerFlag(SpawnerBoolStat stat, bool value)
    {
        switch (stat)
        {
            case SpawnerBoolStat.Disabled: disableEnemySpawning = value; break;
            case SpawnerBoolStat.MakeMeteor: forceEnemySpawning = value; break;
            case SpawnerBoolStat.SpawnSmall: spawnSmall = value; break;
            case SpawnerBoolStat.SpawnRandom: spawnRandom = value; break;
        }
    }

    public void SetHealthFlag(HPBoolStat stat, bool value)
    {
        switch (stat)
        {
            case HPBoolStat.Berserk: berserk = value; break;
            case HPBoolStat.Blunt: blunt = value; break;
            case HPBoolStat.Carving: carving = value; break;
            case HPBoolStat.Meatshield: meatshield = value; break;
            case HPBoolStat.OurL: ouroborosL = value; break;
            case HPBoolStat.OurR: ouroborosR = value; break;
            case HPBoolStat.Porcupine: porcupine = value; break;
            case HPBoolStat.Lethal: lethal = value; break;
            case HPBoolStat.Virgo: virgo = value; break;
            case HPBoolStat.Capricon: capricon = value; break;
        }
    }

    public void SetExperienceFlag(ExpBoolStat stat, bool value)
    {
        hextech = value;
    }

    public void AddRefresh(int amount) => refreshCount += amount;
    public void AddWings(int amount)
    {
        wingCount += amount;

        for (var i = 0; i < amount; i++)
        {
            Instantiate(wingPrefab, player.transform).Initialize(player);
        }
    }
    public void ChangeTimer(int amount) => additionalEnemyCount += amount;
    public void ChangeExperienceRequirement(int amount) => experienceRequirementOffset += amount;
    public void ChangeCoinCoefficient(float value) => coinCoefficient += value;
    public void ChangeItemProbability(float value) => itemProbability += value * 0.01f;
    public void ChangeEnemyDamageCoefficient(float value) => enemyDamageCoefficient *= value;
    public void ChangeBossFatalDamage(float value) => bossFatalDamage = value;
    public void SetWingHoming(bool value) => wingHoming = value;
    public void SetWingFreezing(bool value) => wingFreezing = value;
    public void ChangeWingCooldown(float value) => wingCooldown *= value;
    public void SetCriticalProbabilityFromKills(float value, int limit) { criticalProbabilityPerKill = value; killGrowthLimit = limit; }
    public void SetCriticalDamageFromKills(float value, int limit) { criticalDamagePerKill = value; killGrowthLimit = limit; }
    public void SetDamageCoefficientFromKills(float value, int limit) { damageCoefficientPerKill = value; killGrowthLimit = limit; }
    public void SetDamageFromKills(float value, int limit) { damagePerKill = value; killGrowthLimit = limit; }
    public void SetDamagePerWing(float value) => damagePerWing = value;

    public void ChangeWingDamage(AbilityValueOperation operation, float value)
    {
        wingDamage = operation.Apply(wingDamage, value);
    }

    public void ChangeWingSpeed(AbilityValueOperation operation, float value)
    {
        wingSpeed = operation.Apply(wingSpeed, value);
    }

    public void ApplyCelestialShot(float damageReduction, float fixedDamageGain)
    {
        damageCoefficient *= 1f - damageReduction;
        fixedDamage += fixedDamageGain;
        celestialShot = true;
    }

    public void ApplyPsychosink(float damageReduction, float minimumDamage)
    {
        damageCoefficient *= 1f - damageReduction;
        fixedDamage += minimumDamage;
        allTargetShot = true;
    }

    public void SwapCriticalAndItemProbability()
    {
        var previousCriticalProbability = criticalProbability;
        criticalProbability = itemProbability;
        itemProbability = previousCriticalProbability;
    }

    public float GetFireInterval(float baseInterval)
    {
        return Mathf.Max(0.02f, (baseInterval + fireIntervalOffset) * fireIntervalCoefficient);
    }

    public float GetProjectileSpeed(float baseSpeed) => baseSpeed * shotSpeedCoefficient;

    public ProjectileAbilityData CreateProjectileData(float baseDamage, int basePierce = 0, bool forceHoming = false)
    {
        projectileCount++;
        var growthKills = Mathf.Min(killCount, killGrowthLimit);
        var totalDamageCoefficient = damageCoefficient + (damageCoefficientPerKill * growthKills);
        var totalDamage = ((baseDamage + damage + (damagePerKill * growthKills) + (damagePerWing * wingCount)) * totalDamageCoefficient) + fixedDamage;
        if (third && projectileCount % 3 == 0)
        {
            totalDamage += 0.3f;
        }
        if (berserk)
        {
            totalDamage *= 1f + 1f / player.CurrentHealth;
        }

        if (beingstronger)
        {
            totalDamage *= 1f + Mathf.Min(lastPlayerMoveDistance * 0.02f, 1f);
            lastPlayerMoveDistance = 0f;
        }

        var totalCriticalProbability = criticalProbability + (criticalProbabilityPerKill * growthKills);
        var totalCriticalCoefficient = criticalCoefficient + (criticalDamagePerKill * growthKills);
        if (nuker && totalCriticalProbability > 1f)
        {
            totalCriticalCoefficient += totalCriticalProbability - 1f;
        }

        var isCritical = aquaris || (luckySeven && projectileCount % 7 == 0) || Random.value < totalCriticalProbability;
        if (isCritical)
        {
            totalDamage *= totalCriticalCoefficient;
            if (burst)
            {
                totalDamage += baseDamage;
            }
        }

        var isFatal = Random.value < fatalProbability;
        var pierce = basePierce + (penetrate ? 1 : 0) + (assassination && isCritical ? 1 : 0);
        return new ProjectileAbilityData(totalDamage, isCritical, isFatal, pierce, forceHoming || udo, allTargetShot, freezing, culling, psychosense);
    }

    public ProjectileAbilityData CreateWingProjectileData(float baseDamage, bool homing, bool appliesFreezing)
    {
        var projectileData = CreateProjectileData(baseDamage, 0, homing);
        return new ProjectileAbilityData(
            projectileData.Damage,
            projectileData.IsCritical,
            projectileData.IsFatal,
            projectileData.Pierce,
            projectileData.Homing,
            projectileData.AllTarget,
            projectileData.Freezing || appliesFreezing,
            projectileData.Culling,
            projectileData.Psychosense
        );
    }

    public void OnPlayerDamaged(float damage)
    {
        if (porcupine)
        {
            DamageAllEnemies(damage);
        }
    }

    public void OnBarrierBroken()
    {
        if (blunt) damage += 0.2f;
        if (carving) fireIntervalCoefficient *= 0.9f;
        if (meatshield) player.Heal(10);
        if (ouroborosL) criticalProbability += 0.1f;
        if (ouroborosR) criticalCoefficient += 0.1f;
        if (capricon) TriggerItemEffect();
    }

    public void OnProjectileHit(Enemy enemy, ProjectileAbilityData projectileData)
    {
        if (projectileData.Psychosense && projectileData.IsCritical)
        {
            player.FireAbilityProjectile(player.GetDirectionTo(enemy.transform.position), 3f, 0, false);
        }

        if (projectileData.IsCritical)
        {
            criticalHitCount++;
            if (fifth && criticalHitCount % 5 == 0)
            {
                player.FireAbilityProjectile(player.GetDirectionTo(enemy.transform.position), 1f, 0, false);
            }
        }

        if (enemy.IsDead && projectileData.IsFatal)
        {
            if (kineticBarrage)
            {
                player.FireAbilityProjectile(player.GetDirectionTo(enemy.transform.position), 1f, 0, false);
                player.FireAbilityProjectile(-player.GetDirectionTo(enemy.transform.position), 1f, 0, false);
            }

            if (firm)
            {
                TriggerItemEffect();
            }
        }
    }

    private void HandleEnemyDeath(Enemy enemy)
    {
        killCount++;
        if (noxious)
        {
            DamageAllEnemies(GetCurrentDamage(1f), enemy);
        }

        if (echo)
        {
            player.FireAbilityProjectile(player.GetDirectionTo(enemy.transform.position), 1f, 0, false);
            player.FireAbilityProjectile(-player.GetDirectionTo(enemy.transform.position), 1f, 0, false);
        }

        if (explode)
        {
            for (var i = -2; i <= 2; i++)
            {
                var direction = Quaternion.AngleAxis(i * 15f, Vector3.up) * player.GetDirectionTo(enemy.transform.position);
                player.FireAbilityProjectile(direction, 1f, 0, false);
            }
        }

        if (magnet)
        {
            Enemy.PullAllTowards(enemy.transform.position, 1.5f);
        }

        if (Random.value < itemProbability)
        {
            TriggerItemEffect();
        }
    }

    private void HandleItemCollected(Player collectingPlayer)
    {
        if (reinforce) criticalProbability += 0.1f;
        if (jera) damage += 0.2f;
        if (dagaz) player.AddBarrier(1);
        if (repair) player.Heal(10);
        if (hextech) refreshCount++;
    }

    private void TriggerItemEffect()
    {
        HandleItemCollected(player);
    }

    private void TriggerPeriodicAbility(int abilityId)
    {
        switch (abilityId)
        {
            case 2: player.FireAbilityProjectile(player.GetNearestEnemyDirection(), 1f, 0, true); break;
            case 3: player.FireAbilityProjectile(player.GetNearestEnemyDirection(), 1f, 0, false); break;
            case 17: DamageAllEnemies(GetCurrentDamage(1f)); break;
            case 19: player.FireAbilityProjectile(player.GetNearestEnemyDirection(), 1f, 0, false); break;
            case 21: player.FireFatalProjectile(player.GetNearestEnemyDirection()); break;
            case 24: DamageAllEnemies(4f); break;
            case 43: fixedDamage += 0.05f; break;
            case 45: player.Heal(5); break;
            case 47: FireRadialProjectiles(6, 6f); break;
            case 55: Enemy.SlowAll(0.75f, 3f); break;
            case 63: player.AddBarrier(1); break;
            case 66: criticalProbability += 0.025f; break;
            case 70: criticalCoefficient += 0.025f; break;
            case 77: FireHomingProjectiles(3, 1f); break;
            case 79: player.FireAbilityProjectile(player.GetNearestEnemyDirection(), 3f, 1, false); break;
            case 80: StartCoroutine(Rearm()); break;
            case 84: damage += 0.1f; break;
            case 91: DamageAllEnemies(GetCurrentDamage(1f)); break;
            case 96: player.FireAbilityProjectile(player.GetNearestEnemyDirection(), GetCurrentDamage(2f), 0, false); break;
            case 98: player.FireCriticalProjectile(player.GetNearestEnemyDirection()); break;
            case 114: fireIntervalOffset -= 0.002f; break;
            case 135: TriggerItemEffect(); break;
        }
    }

    private System.Collections.IEnumerator Rearm()
    {
        canFire = false;
        yield return new WaitForSeconds(0.5f);
        canFire = true;
    }

    private void FireRadialProjectiles(int count, float projectileDamage)
    {
        for (var i = 0; i < count; i++)
        {
            var direction = Quaternion.Euler(0f, 360f * i / count, 0f) * Vector3.forward;
            player.FireAbilityProjectile(direction, projectileDamage, 0, false);
        }
    }

    private void FireHomingProjectiles(int count, float projectileDamage)
    {
        for (var i = 0; i < count; i++)
        {
            player.FireAbilityProjectile(player.GetNearestEnemyDirection(), projectileDamage, 0, true);
        }
    }

    private float GetCurrentDamage(float baseDamage)
    {
        return CreateProjectileData(baseDamage).Damage;
    }

    private static void DamageAllEnemies(float damage, Enemy excludedEnemy = null)
    {
        Enemy.DamageAll(damage, excludedEnemy);
    }

    private void AddSynergy(int synergyType)
    {
        Synergy[synergyType]++;
    }

    private Ability CreateAbility(AbilityData abilityData)
    {
        var abilityObject = new GameObject(abilityData.name);
        abilityObject.transform.SetParent(transform);
        var ability = abilityObject.AddComponent<Ability>();
        ability.AbilityID = abilityData.AbilityID;
        ability.AbilityType = abilityData.AbilityType;
        return ability;
    }
}

public sealed class ProjectileAbilityData
{
    public ProjectileAbilityData(float damage, bool isCritical, bool isFatal, int pierce, bool homing, bool allTarget, bool freezing, bool culling, bool psychosense)
    {
        Damage = damage;
        IsCritical = isCritical;
        IsFatal = isFatal;
        Pierce = pierce;
        Homing = homing;
        AllTarget = allTarget;
        Freezing = freezing;
        Culling = culling;
        Psychosense = psychosense;
    }

    public float Damage { get; }
    public bool IsCritical { get; }
    public bool IsFatal { get; }
    public int Pierce { get; }
    public bool Homing { get; }
    public bool AllTarget { get; }
    public bool Freezing { get; }
    public bool Culling { get; }
    public bool Psychosense { get; }
}
