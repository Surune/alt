using System;
using UnityEngine;

[Serializable]
public class AbilityEffect
{
    [SerializeField] AbilityEffectKind kind;
    [SerializeField] AbilityManagerFlag abilityManagerFlag;
    [SerializeField] AbilityValueOperation operation;
    [SerializeField] PlayerManagerFloatStat playerManagerFloatStat;
    [SerializeField] PlayerManagerBoolStat playerManagerBoolStat;
    [SerializeField] SpawnerFloatStat spawnerFloatStat;
    [SerializeField] SpawnerBoolStat spawnerBoolStat;
    [SerializeField] HPBoolStat hpBoolStat;
    [SerializeField] ExpBoolStat expBoolStat;
    [SerializeField] float value;
    [SerializeField] float secondValue;
    [SerializeField] int amount;
    [SerializeField] int secondAmount;
    [SerializeField] bool enabledValue = true;

    public void Apply(AbilityApplyContext context)
    {
        switch (kind)
        {
            case AbilityEffectKind.SetAbilityCooldown:
                context.Ability.SkillCooltimeMax = value;
                context.AbilityManager.RegisterPeriodicAbility(context.AbilityData.AbilityID, value);
                break;
            case AbilityEffectKind.ChangePlayerSkillCooldown:
                context.AbilityManager.ChangeFireInterval(operation, value);
                break;
            case AbilityEffectKind.SetAbilityManagerFlag:
                context.AbilityManager.SetFlag(abilityManagerFlag);
                break;
            case AbilityEffectKind.PlayerManagerFloat:
                context.AbilityManager.ChangePlayerStat(playerManagerFloatStat, operation, value);
                break;
            case AbilityEffectKind.PlayerManagerBool:
                context.AbilityManager.SetPlayerFlag(playerManagerBoolStat, enabledValue);
                break;
            case AbilityEffectKind.PlayerManagerRefresh:
                context.AbilityManager.AddRefresh(amount);
                break;
            case AbilityEffectKind.PlayerManagerWing:
                context.AbilityManager.AddWings(amount);
                break;
            case AbilityEffectKind.SpawnerFloat:
                context.AbilityManager.ChangeSpawnerStat(spawnerFloatStat, operation, value);
                break;
            case AbilityEffectKind.SpawnerBool:
                context.AbilityManager.SetSpawnerFlag(spawnerBoolStat, enabledValue);
                break;
            case AbilityEffectKind.HPBarrier:
                context.Player.AddBarrier(amount);
                break;
            case AbilityEffectKind.HPBool:
                context.AbilityManager.SetHealthFlag(hpBoolStat, enabledValue);
                break;
            case AbilityEffectKind.ChangeHP:
                context.Player.ChangeMaxHealthByPercent(-amount * 0.01f);
                break;
            case AbilityEffectKind.ChangeHPValues:
                context.Player.ChangeHealthScale(value, secondValue);
                break;
            case AbilityEffectKind.RefreshHealthBar:
                context.Player.HealToFull();
                break;
            case AbilityEffectKind.Timer:
                context.AbilityManager.ChangeTimer(amount);
                break;
            case AbilityEffectKind.ExpBool:
                context.AbilityManager.SetExperienceFlag(expBoolStat, enabledValue);
                break;
            case AbilityEffectKind.ExpMax:
                context.AbilityManager.ChangeExperienceRequirement(amount);
                break;
            case AbilityEffectKind.CoinCoefficient:
                context.AbilityManager.ChangeCoinCoefficient(value);
                break;
            case AbilityEffectKind.EnemyItemProb:
                context.AbilityManager.ChangeItemProbability(value);
                break;
            case AbilityEffectKind.EnemyDamageCoefficient:
                context.AbilityManager.ChangeEnemyDamageCoefficient(value);
                break;
            case AbilityEffectKind.FireballFatalDamage:
                context.AbilityManager.ChangeBossFatalDamage(value);
                break;
            case AbilityEffectKind.WingBulletDamage:
                context.AbilityManager.ChangeWingDamage(operation, value);
                break;
            case AbilityEffectKind.WingBulletSpeed:
                context.AbilityManager.ChangeWingSpeed(operation, value);
                break;
            case AbilityEffectKind.WingBulletUdo:
                context.AbilityManager.SetWingHoming(true);
                break;
            case AbilityEffectKind.WingFreezing:
                context.AbilityManager.SetWingFreezing(true);
                break;
            case AbilityEffectKind.ChangeAllWingCooldown:
                context.AbilityManager.ChangeWingCooldown(value);
                break;
            case AbilityEffectKind.CriticalProbFromKills:
                context.AbilityManager.SetCriticalProbabilityFromKills(value, amount);
                break;
            case AbilityEffectKind.CriticalCoefficientFromKills:
                context.AbilityManager.SetCriticalDamageFromKills(value, amount);
                break;
            case AbilityEffectKind.DamageCoefficientFromKills:
                context.AbilityManager.SetDamageCoefficientFromKills(value, amount);
                break;
            case AbilityEffectKind.DamageFromKills:
                context.AbilityManager.SetDamageFromKills(value, amount);
                break;
            case AbilityEffectKind.CelestialShot:
                context.AbilityManager.ApplyCelestialShot(value, secondValue);
                break;
            case AbilityEffectKind.Psychosink:
                context.AbilityManager.ApplyPsychosink(value, secondValue);
                break;
            case AbilityEffectKind.LibraSwap:
                context.AbilityManager.SwapCriticalAndItemProbability();
                break;
            case AbilityEffectKind.Berkano:
                context.AbilityManager.SetDamagePerWing(value);
                break;
            case AbilityEffectKind.ChooseRandomAbilities:
                context.AbilityManager.ChooseRandomAbilities(amount);
                break;
        }
    }
}
